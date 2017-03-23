using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PHttp
{
    internal class HttpClient : IDisposable
    {

        private bool _disposed;
        private ClientState _state;
        private HttpContext _context;
        private readonly byte[] _writeBuffer;
        private NetworkStream _stream;
        private MemoryStream _writeStream;
        private bool _errored;
        private HttpRequestParser _parser;

        private static readonly Regex PrologRegex = new Regex("^([A-Z]+) ([^ ]+) (HTTP/[^ ]+)$", RegexOptions.Compiled);

        public HttpServer Server { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public HttpReadBuffer ReadBuffer { get; private set; }
        public Stream InputStream { get; set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Method { get; private set; }
        public string Protocol { get; private set; }
        public string Request { get; private set; }
        public List<HttpMultiPartItem> MultiPartItems { get; set; }
        public NameValueCollection PostParameters { get; set; }

        private void BeginRead()
        {
            if (_disposed)
                return;
            try
            {
                // Reads should be within a certain timeframe

                Server.TimeoutManager.ReadQueue.Add(
                    ReadBuffer.BeginRead(_stream, ReadCallback, null),
                    this
                );
            }
            catch (Exception ex)
            {
                Dispose();
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            if (_disposed)
                return;
            if (_state == ClientState.ReadingProlog && Server.State != HttpServerState.Started)
            {
                Dispose();
                return;
            }

            try
            {
                ReadBuffer.EndRead(InputStream, asyncResult);
                if (ReadBuffer.DataAvailable)
                {
                    ProcessReadBuffer();
                }
                else
                {
                    Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
                Dispose();
            }
            catch (Exception e)
            {
                ProcessException(e);
            }
        }

        private void ProcessException(Exception exception)
        {

        }

        private void ProcessReadBuffer()
        {
            if (_disposed) return;

            while (ReadBuffer.DataAvailable && _writeStream == null)
            {
                switch (_state)
                {
                    case ClientState.ReadingProlog:
                        ProcessProlog();
                        break;
                    case ClientState.ReadingHeaders:
                        ProcessHeaders();
                        break;
                    case ClientState.ReadingContent:
                        ProcessContent();
                        break;
                }
            }

            if (_writeStream == null) BeginRead();
        }

        private void ProcessProlog()
        {
            var readBuffer = ReadBuffer.ReadLine();
            if (string.IsNullOrEmpty(readBuffer)) return;

            var matchResults = PrologRegex.Matches(readBuffer);

            //TODO check for outofrange
            if (!matchResults[0].Success || !matchResults[1].Success || !matchResults[1].Success)
            {
                throw new ProtocolException("The prolog could not be parsed: " + readBuffer);
            }

            Method = matchResults[0].Value;
            Request = matchResults[1].Value;
            Protocol = matchResults[2].Value;

            _state = ClientState.ReadingHeaders;

            ProcessHeaders();
        }

        private void ProcessHeaders()
        {
            string line = null;
            while ((line = ReadBuffer.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    ReadBuffer.Reset();
                    _state = ClientState.ReadingContent;
                    ProcessContent();
                    return;
                }

                var parts = line.Split(new[] { ':' }, 2);

                if (parts.Length != 2)
                {
                    throw new ProtocolException("Received header without colon");
                }

                Headers[parts[0].Trim()] = parts[1].Trim();
            }
        }

        private void ProcessContent()
        {
            if (_parser != null)
            {
                _parser.Parse();
                return;
            }

            if (!ProcessExpectHeader() && !ProcessContentLengthHeader())
            {
                ExecuteRequest();
            }
        }

        private void ExecuteRequest()
        {

        }

        private void Reset()
        {
            _state = ClientState.ReadingProlog;
            _context = null;
            if (_parser != null)
            {
                _parser.Dispose();
                _parser = null;
            }

            if (_writeStream != null)
            {
                _writeStream.Dispose();
                _writeStream = null;
            }

            if (InputStream != null)
            {
                InputStream.Dispose();
                InputStream = null;
            }

            ReadBuffer.Reset();
            Method = null;
            Protocol = null;
            Request = null;
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            PostParameters = new NameValueCollection();

            if (MultiPartItems != null)
            {
                foreach (var item in MultiPartItems)
                {
                    if (item.Stream != null)
                        item.Stream.Dispose();
                }

                MultiPartItems = null;
            }
        }

        public void RequestClose()
        {
            if (_state == ClientState.ReadingProlog)
            {
                var stream = _stream;
                if (stream != null)
                    stream.Dispose();
            }
        }

        public void ForceClose()
        {
            var stream = _stream;
            if (stream != null)
                stream.Dispose();
        }

        public void UnsetParser()
        {
            Debug.Assert(_parser != null);
            _parser = null;
        }

        private bool ProcessExpectHeader()
        {
            // Process the Expect: 100-continue header.
            string expectHeader;
            if (Headers.TryGetValue("Expect", out expectHeader))
            {
                // Remove the expect header for the next run.
                Headers.Remove("Expect");
                int pos = expectHeader.IndexOf(';');
                if (pos != -1)
                    expectHeader = expectHeader.Substring(0, pos).Trim();
                if (!String.Equals("100-continue", expectHeader, StringComparison.OrdinalIgnoreCase))
                    throw new ProtocolException(String.Format("Could not process Expect header '{0}'", expectHeader));
                SendContinueResponse();
                return true;
            }
            return false;
        }

        private bool ProcessContentLengthHeader()
        {
            // Read the content.
            string contentLengthHeader;
            if (Headers.TryGetValue("Content-Length", out contentLengthHeader))
            {
                int contentLength;
                if (!int.TryParse(contentLengthHeader, out contentLength))
                    throw new ProtocolException(String.Format("Could not parse Content-Length header '{0}'", contentLengthHeader));
                string contentTypeHeader;
                string contentType = null;
                string contentTypeExtra = null;
                if (Headers.TryGetValue("Content-Type", out contentTypeHeader))
                {
                    string[] parts = contentTypeHeader.Split(new[] { ';' }, 2);
                    contentType = parts[0].Trim().ToLowerInvariant();
                    contentTypeExtra = parts.Length == 2 ? parts[1].Trim() : null;
                }
                if (_parser != null)
                {
                    _parser.Dispose();
                    _parser = null;
                }
                switch (contentType)
                {
                    case "application/x-www-form-urlencoded":
                        _parser = new HttpUrlEncodedRequestParser(this, contentLength);
                        break;
                    case "multipart/form-data":
                        string boundary = null;
                        if (contentTypeExtra != null)
                        {
                            string[] parts = contentTypeExtra.Split(new[] { '=' }, 2);
                            if (
                                parts.Length == 2 &&
                                String.Equals(parts[0], "boundary", StringComparison.OrdinalIgnoreCase)
                            )
                                boundary = parts[1];
                        }
                        if (boundary == null)
                            throw new ProtocolException("Expected boundary with multipart content type");
                        _parser = new HttpMultiPartRequestParser(this, contentLength, boundary);
                        break;
                    default:
                        _parser = new HttpUnknownRequestParser(this, contentLength);
                        break;
                }
                // We've made a parser available. Recurs back to start processing
                // with the parser.
                ProcessContent();
                return true;
            }
            return false;
        }

        private void SendContinueResponse()
        {
            var sb = new StringBuilder();
            sb.Append(Protocol);
            sb.Append(" 100 Continue\r\nServer: ");
            sb.Append(Server.ServerBanner);
            sb.Append("\r\nDate: ");
            sb.Append(DateTime.UtcNow.ToString("R"));
            sb.Append("\r\n\r\n");
            var bytes = Encoding.ASCII.GetBytes(sb.ToString());
            if (_writeStream != null)
                _writeStream.Dispose();
            _writeStream = new MemoryStream();
            _writeStream.Write(bytes, 0, bytes.Length);
            _writeStream.Position = 0;
            //BeginWrite();
        }

        private enum ClientState
        {
            ReadingProlog,
            ReadingHeaders,
            ReadingContent,
            WritingHeaders,
            WritingContent,
            Closed
        }

        public void Dispose()
        {
            _state = ClientState.ReadingProlog;
            _context = null;
            if (_parser != null)
            {
                _parser.Dispose();
                _parser = null;
            }

            if (_writeStream != null)
            {
                _writeStream.Dispose();
                _writeStream = null;
            }

            if (InputStream != null)
            {
                InputStream.Dispose();
                InputStream = null;
            }

            ReadBuffer.Reset();
            Method = null;
            Protocol = null;
            Request = null;
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            PostParameters = new NameValueCollection();

            if (MultiPartItems != null)
            {
                foreach (var item in MultiPartItems)
                {
                    if (item.Stream != null)
                        item.Stream.Dispose();
                }

                MultiPartItems = null;
            }
        }
    }
}

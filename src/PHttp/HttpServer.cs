using System;
using System.Net;

namespace PHttp
{
    public class HttpServer : IDisposable
    {
        private const int WriteReadBufferSize = 4096;
        private const int ShutdownTimeout = 30;
        private const int WriteReadTimeOut = 90;

        #region Properties
        public IPEndPoint EndPoint { get; set; }
        public int ReadBufferSize { get; set; }
        public int WriteBufferSize { get; set; }
        public string ServerBanner { get; set; }
        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }
        public TimeSpan ShutdowTimeout { get; set; }

       // internal HttpServerUtility ServerUtility { get; set; }
      //  internal HttpTimeoutManager TimeoutManager { get; set; }
        #endregion

        #region Constructor
        public HttpServer()
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            ReadBufferSize = WriteReadBufferSize;
            WriteBufferSize = WriteReadBufferSize;
            ShutdowTimeout = TimeSpan.FromSeconds(ShutdownTimeout);
            ReadTimeout = TimeSpan.FromMinutes(WriteReadTimeOut);
            WriteTimeout = TimeSpan.FromMinutes(WriteReadTimeOut);
            ServerBanner = string.Format("PHttp/{0}", GetType().Assembly.GetName().Version);
        }

        #endregion

        #region Public Methods

        public void Start()
        {

        }

        public void Stop()
        {

        }

        #endregion

        #region Private/Internal Methods
        private void VerifyState(HttpServerState state)
        {

        }

        private void StopClients()
        {

        }

        private void BeginAcceptTcpClient()
        {

        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {

        }

        private void RegisterClient(HttpClient client)
        {

        }

        internal void UnregisterClient(HttpClient client)
        {

        }
        #endregion

        #region Implemented Methods
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

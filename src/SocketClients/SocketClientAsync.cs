using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketClients
{
    public class SocketClientAsync
    {
        public const int BufferSize = 256;

        private readonly IPEndPoint _ipEndPoint;
        private readonly Socket _sender;

        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent _receiveDone =
            new ManualResetEvent(false);

        public SocketClientAsync(string ipAddress, int port)
        {
            IPAddress ipAddressParsed;

            if (IPAddress.TryParse(ipAddress, out ipAddressParsed))
            {
                _ipEndPoint = new IPEndPoint(ipAddressParsed, port);
            }
            else
            {
                throw new ArgumentException("Invalid ip", ipAddress);
            }

            _sender = new Socket(AddressFamily.InterNetwork,
                  SocketType.Stream, ProtocolType.Tcp);
        }

        public void StartConnectionAsync(Action<bool> asyncCallback)
        {
            if (!_sender.Connected)
            {
                _sender.BeginConnect(_ipEndPoint, (res) =>
                {
                    asyncCallback(_sender.Connected);

                }, _sender);
            }

        }

        public void SendMessageAsync(string message)
        {
            if (message == null) return;
            if (!_sender.Connected)
            {
                Console.WriteLine("Start connection first");
                return;
            }

            var msg = Encoding.ASCII.GetBytes(message);
            _sender.BeginSend(msg, 0, msg.Length, 0, (res) =>
            {
                ((Socket)res.AsyncState).EndSend(res);
            }, _sender);
        }

        public void ReceiveMessageAsync(Action<string> receiveCallback)
        {
            try
            {
                var buffer = new byte[BufferSize];
                _sender.BeginReceive(buffer, 0, BufferSize, 0, (res) =>
                {
                    ReceiveCallback(res, buffer, receiveCallback);

                }, _sender);

                _receiveDone.WaitOne();
                _receiveDone.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private void ReceiveCallback(IAsyncResult res, byte[] buffer, Action<string> receiveCallBack)
        {
            try
            {
                var bytesRead = ((Socket)res.AsyncState).EndReceive(res);

                if (bytesRead > 0)
                {
                    receiveCallBack(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                    ((Socket)res.AsyncState).BeginReceive(buffer, 0, BufferSize, 0,
                        (ar) => ReceiveCallback(ar, buffer, receiveCallBack), _sender);
                }
                else
                {
                  //  if (messageReceivedBuilder.Length <= 1) return;
                   // receiveCallBack(messageReceivedBuilder.ToString());
                    //_receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void CloseConnection()
        {
            if (_sender.Connected)
                _sender.Shutdown(SocketShutdown.Both);

            _sender.Close();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketClients
{
    public class SocketClientSync
    {
        private readonly IPEndPoint _ipEndPoint;
        private readonly Socket _sender;

        public SocketClientSync(string ipAddress, int port)
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

        public void StartConnection()
        {
            if(!_sender.Connected)
            _sender.Connect(_ipEndPoint);
        }

        public string SendAndReceiveMessage(string message)
        {
            if (message == null) return null;
            if (!_sender.Connected)
            {
              Console.WriteLine("Start connection first");
                return null;
            }

            var bytes = new byte[1024];

            var msg = Encoding.ASCII.GetBytes(message);
            _sender.Send(msg);

            var recMsg = _sender.Receive(bytes);
            return Encoding.ASCII.GetString(bytes, 0, recMsg);
        }

        public void CloseConnection()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClients
{
    class Program
    {
        private static readonly SocketClientAsync SocketClient = new SocketClientAsync("192.168.100.10", 8083);
        private static readonly ManualResetEvent _finish =
     new ManualResetEvent(false);

        static void Main(string[] args)
        {

            SocketClient.StartConnectionAsync(SocketConnectedCallback);
            _finish.WaitOne();

      SocketClient.CloseConnection();

        }

        private static void SocketConnectedCallback(bool success)
        {
            if (success)
            {
                Task.Run(() => SocketClient.ReceiveMessageAsync(ReceiveMessageCallback));  

                while (true)
                {
                    SocketClient.SendMessageAsync(Console.ReadLine());
                }

            }

            else
                Console.WriteLine("there was an error establishing the connection");
        }

        private static void SendMessageCallback(int byteSent)
        {
            Console.WriteLine("bytes sent: " + byteSent);
        }

        private static void ReceiveMessageCallback(string result)
        {
            Console.Write("message received: " + result);
        }
    }
}

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverAddress = "192.168.6.16";
            int serverPort = 500;
            TcpClient server;

            ActivateListener();

            while (true)
            {
                string str = Console.ReadLine();

                server = new TcpClient(serverAddress, serverPort);
                NetworkStream serverStream = server.GetStream();
                byte[] stringToByte = Encoding.ASCII.GetBytes(str);
                serverStream.Write(stringToByte, 0, stringToByte.Length);

                //receiving
                /*byte[] receive = new byte[server.ReceiveBufferSize];
                serverStream.Read(receive, 0, server.ReceiveBufferSize);
                Console.WriteLine(Encoding.ASCII.GetString(receive));*/
                server.Close();
            }

        }

        private static async Task ActivateListener()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 500);
            listener.Start();
            while (true)
            {
                await Task.Run(()=> {
                    Socket serverSocket = listener.AcceptSocket();
                    byte[] messageStream = new byte[serverSocket.ReceiveBufferSize];
                    serverSocket.Receive(messageStream);
                    string message = Encoding.ASCII.GetString(messageStream).Trim('\0');

                    Console.WriteLine(message);

                    serverSocket.Close();
                });
            }
        }
    }
}

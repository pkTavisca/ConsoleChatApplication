using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp2
{
    class Program
    {
        public static HashSet<string> IP_Addresses = new HashSet<string>();
        static void Main(string[] args)
        {
            //Lambda.PatternThroughLambda();

            /*AsyncAndAwait aaa = new AsyncAndAwait();
            int a = aaa.SomeMethod();
            Console.WriteLine(a);*/

            string myIP = "192.168.6.16";

            IPAddress localAddress = IPAddress.Parse(myIP);
            TcpListener server = new TcpListener(localAddress, 500);

            server.Start();

            while (true)
            {
                Socket clientSocket = server.AcceptSocket();
                byte[] incomingStream = new byte[2048];
                clientSocket.Receive(incomingStream);

                string message = Encoding.ASCII.GetString(incomingStream).Trim('\0');

                string clientIP = (clientSocket.RemoteEndPoint as IPEndPoint).Address.ToString();
                IP_Addresses.Add(clientIP);
                Console.WriteLine("IP: {0} | {1}", clientIP, message);

                //logging
                using (StreamWriter sw = new StreamWriter("chatLogs.txt", true))
                {
                    sw.WriteLine("IP: {0} | {1}", clientIP, message);
                }

                //sending to all other clients
                foreach (string ip in IP_Addresses)
                {
                    if (!ip.Equals(clientIP))
                    {
                        TcpClient tcpClient = new TcpClient(ip, 500);
                        NetworkStream tcpStream = tcpClient.GetStream();
                        tcpStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                        tcpClient.Close();
                    }
                }

                clientSocket.Close();
                clientSocket.Dispose();
            }
        }
    }
}

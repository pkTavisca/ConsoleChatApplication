//using System;
//using System.Text;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading.Tasks;
//using System.Linq;
//using System.Threading;

//namespace ClientApp
//{
//    class Program
//    {
//        static TcpListener listener;
//        static void Main(string[] args)
//        {
//            string serverAddress = "192.168.6.16";
//            int serverPort = 500;
//            TcpClient server;

//            Console.Write("Enter your name: ");
//            string name = Console.ReadLine();
//            name = new string(name.Where(char.IsLetter).ToArray());

//            while (true)
//            {
//                string messageToSend = string.Format("{0}:\t{1}", name, Console.ReadLine());
//                server = new TcpClient(serverAddress, serverPort);
//                int clientPort;
//                int.TryParse((server.Client.LocalEndPoint as IPEndPoint).Port.ToString(), out clientPort);
//                CancellationTokenSource tokenSource = new CancellationTokenSource();
//                if (listener != null)
//                { tokenSource.Cancel(); listener.Stop(); }
//                try
//                {
//                    listener = new TcpListener(IPAddress.Any, clientPort);
//                    listener.Start();
//                }
//                catch (Exception e) { Console.WriteLine(e.Message); }

//                ActivateListener(tokenSource.Token);



//                NetworkStream serverStream = server.GetStream();
//                byte[] stringToByte = Encoding.ASCII.GetBytes(messageToSend);
//                serverStream.Write(stringToByte, 0, stringToByte.Length);
//                server.Close();
//            }

//        }

//        private static async Task ActivateListener(CancellationToken cancellationToken)
//        {
//            while (cancellationToken.IsCancellationRequested == false)
//            {
//                await Task.Run(() =>
//                {
//                    try
//                    {
//                        //Console.WriteLine("inside async block");
//                        Socket serverSocket = listener.AcceptSocket();
//                        byte[] messageStream = new byte[serverSocket.ReceiveBufferSize];
//                        serverSocket.Receive(messageStream);
//                        string message = Encoding.ASCII.GetString(messageStream).Trim('\0');

//                        Console.WriteLine(message);

//                        serverSocket.Close();
//                    }
//                    catch (Exception e) { }
//                }, cancellationToken);
//            }
//        }
//    }
//}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class Client
    {
        public Client(string ipAddress, int port)
        {
            this.IPAddress = ipAddress;
            this.Port = port;
        }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public string Alias { get; set; }
    }

    class Program
    {
        static string myIP = "192.168.6.16";
        static IPAddress localAddress = IPAddress.Parse(myIP);
        static TcpListener server = new TcpListener(localAddress, 500);

        public static List<Client> lstClient = new List<Client>();
        public static List<TcpClient> setOfTcpClients = new List<TcpClient>();
        public static List<KeyValuePair<string, string>> _ipToNameMapping = new List<KeyValuePair<string, string>>();

        static void Main(string[] args)
        {
            server.Start();

            while (true)
            {
                //TODO: determine multiple clients or single client
                TcpClient newClient = server.AcceptTcpClient();
                Console.WriteLine("{0} connected..", (newClient.Client.RemoteEndPoint as IPEndPoint).Address);
                setOfTcpClients.Add(newClient);

                Task.Run(() =>
                {
                    while (true)
                    {
                        ReadMessage();
                    }
                });
            }

            //chatting loop
            //while (true)
            //{


            ////var client = HandleClient(clientSocket);

            //LogMessage(client, message);



            ////logging


            ////sending to all other clients
            //Console.Write("Sending To: ");
            //foreach (var client in lstClient)
            //{
            //    if (!client.IPAddress.Equals(clientIP))
            //    {
            //        try
            //        {
            //            TcpClient tcpClient = new TcpClient(client.IPAddress, client.Port);
            //            NetworkStream tcpStream = tcpClient.GetStream();
            //            tcpStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
            //            Console.Write("{0}:{1},", client.IPAddress, client.Port);
            //            tcpClient.Close();
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine(e.Message);
            //            continue;
            //        }
            //    }
            //}
            //Console.WriteLine();

            //clientSocket.Close();
            //clientSocket.Dispose();
            //}
        }
        private static bool ContainsKey(string key)
        {
            return _ipToNameMapping.Any(kvp => string.Equals(kvp.Key, key, StringComparison.InvariantCultureIgnoreCase));
        }
        private static void ReadMessage()
        {
            for (int i = 0; i < setOfTcpClients.Count; i++)
            {

                try
                {
                    var item = setOfTcpClients[i];
                    if (item == null)
                        continue;
                    var ip = (item.Client.RemoteEndPoint as IPEndPoint).Address.ToString();

                    if (!item.Connected) continue;
                    NetworkStream clientStream = item.GetStream();
                    if (!clientStream.DataAvailable)
                    {
                        continue;
                    }
                    byte[] clientByteStream = new byte[2048];
                    clientStream.Read(clientByteStream, 0, clientByteStream.Length);
                    string message = Encoding.ASCII.GetString(clientByteStream).Trim('\0');
                    //Console.WriteLine(message);
                    if (ContainsKey(ip) == false)
                        _ipToNameMapping.Add(new KeyValuePair<string, string>(ip, ip));

                    if (message.ToLowerInvariant().StartsWith("name:"))
                    {
                        var name = message.Split(':')[1];
                        _ipToNameMapping.RemoveAll(k => string.Equals(k.Key, ip));
                        _ipToNameMapping.Add(new KeyValuePair<string, string>(ip, name));
                    }
                    else if (message.ToLowerInvariant().StartsWith("get:"))
                    {
                        var clients = string.Empty;
                        foreach (var key in _ipToNameMapping)
                        {
                            clients += key.Value + " ";
                        }

                        Broadcast(clients, string.Empty, ip);
                    }
                    else if(message.ToLowerInvariant().StartsWith("to:"))
                    {
                        var to = message.Split(' ')[0].Split(':')[1];
                        message = message.Replace("to:" + to, "");
                        var kvp = _ipToNameMapping.FirstOrDefault(k => string.Equals(k.Value, to, StringComparison.InvariantCultureIgnoreCase));
                        if (string.IsNullOrWhiteSpace(kvp.Key) == false)
                        {
                            Broadcast(message, kvp.Value, kvp.Key);
                        }
                        
                    }
                    else
                    {
                        var kvp = _ipToNameMapping.FirstOrDefault(k => string.Equals(k.Key, ip));
                        Broadcast(message, kvp.Value,  null);
                    }
                }
                catch (Exception)
                {

                }

            }
        }

        private static void Broadcast(string message, string name, string ip)
        {
            if (string.IsNullOrWhiteSpace(name) == false)
                message = string.Format("[{0}]:{1}\n", name, message);
            
            Console.WriteLine(message);
            var x = new List<TcpClient>(setOfTcpClients);

            if (string.IsNullOrWhiteSpace(ip) == false)
            {
                var client = setOfTcpClients.FirstOrDefault(c => string.Equals((c.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), ip));
                if(client != null && client.Connected)
                {
                    Process(client, message);
                    return;
                }
            }
            
            //Console.WriteLine(message);
            foreach (var client in x)
            {
                Process(client, message);
            }
        }

        private static void Process(TcpClient client, string message)
        {
            if (!client.Connected) return;
            NetworkStream clientStream = client.GetStream();
            try
            {
                clientStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }
        }

        private static Client HandleClient(Socket socket)
        {
            string clientIP = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            int clientPort;
            int.TryParse((socket.RemoteEndPoint as IPEndPoint).Port.ToString(), out clientPort);

            if (lstClient.FirstOrDefault(client => string.Equals(client.IPAddress, clientIP)) == null)
            {
                lstClient.Add(new Client(clientIP, clientPort));
            }

            return new Client(clientIP, clientPort);
        }

        private static void LogMessage(Client client, string message)
        {
            Console.WriteLine("IP: {0}:{1} | {2}", client.IPAddress, client.Port, message);

            using (StreamWriter sw = new StreamWriter("chatLogs.txt", true))
            {
                sw.WriteLine("IP: {0}:{1} | {2}", client.IPAddress, client.Port, message);
            }
        }

    }
}

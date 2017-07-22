using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp2
{
    class Program
    {
        static Dictionary<Client, TcpClient> listOfConnectedClients = new Dictionary<Client, TcpClient>();
        static int serverPort = 500;

        public static void Main(string[] args)
        {
            TcpListener serverListener = new TcpListener(GetOwnIP(), serverPort);
            serverListener.Start();

            //loop for adding new clients
            Task.Run(() =>
            {
                while (true)
                {
                    TcpClient newClient = serverListener.AcceptTcpClient();
                    string newClientIp = GetIpOfClient(newClient).ToString();
                    bool clientFound = false;
                    foreach (var connectedClient in listOfConnectedClients)
                    {
                        if (connectedClient.Key.IpAddess.ToString().Equals(newClientIp))
                        {
                            connectedClient.Value.Close();
                            listOfConnectedClients[connectedClient.Key] = newClient;
                            Console.WriteLine("Replaced Connection : {0}",
                                connectedClient.Key.ToString());
                            clientFound = true;
                            break;
                        }
                    }
                    if (!clientFound)
                    {
                        Client clientToAdd = new Client(newClientIp);
                        Console.WriteLine("New Connection: {0}", newClientIp);
                        listOfConnectedClients[clientToAdd] = newClient;
                    }
                }
            });

            //chatting loop
            while (true)
            {
                //cloning
                var cloneOfConnectedClients = cloneDictionary(listOfConnectedClients);
                foreach (var clonedClient in cloneOfConnectedClients)
                {
                    if (clonedClient.Value.Connected)
                    {
                        NetworkStream stream = clonedClient.Value.GetStream();
                        try
                        {
                            if (stream.DataAvailable)
                            {
                                byte[] streamStorage = new byte[4096];
                                stream.Read(streamStorage, 0, streamStorage.Length);
                                Task.Run(() =>
                                {
                                    ProcessIncomingMessage(Encoding.ASCII.GetString(streamStorage).Trim('\0'), clonedClient.Key);
                                });
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private static void Brodcast(string message, Client from)
        {
            var cloneOfConnectedClients = cloneDictionary(listOfConnectedClients);
            message = (from.ToString())
                + " : " + message;
            message = message.Trim('\0');
            Console.WriteLine(message);
            foreach (var cloneClient in cloneOfConnectedClients)
            {
                if (cloneClient.Value.Connected)
                {
                    try
                    {
                        NetworkStream stream = cloneClient.Value.GetStream();
                        stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                    }
                    catch { }
                }
            }
        }

        private static void ProcessIncomingMessage(string message, Client from)
        {
            if (message.ToLowerInvariant().StartsWith("to:"))
            {
                message = message.Substring("to:".Length);
                string to = message.Split(' ')[0];
                message = message.Substring(to.Length);
                message = string.Format("[Private Message from {0}]: {1}", from.ToString(), message);
                var clientsClone = cloneDictionary(listOfConnectedClients);
                foreach (var client in clientsClone)
                {
                    if (client.Key.ToString().Equals(to))
                    {
                        NetworkStream stream = client.Value.GetStream();
                        stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                        Console.WriteLine("To:{0} {1}", to, message);
                        break;
                    }
                }
            }
            else if (message.ToLowerInvariant().StartsWith("get:"))
            {
                message = "";
                var clientsClone = cloneDictionary(listOfConnectedClients);
                foreach (var client in clientsClone)
                {
                    if (!from.ToString().Equals(client.Key.ToString()))
                        message += client.Key.ToString() + " ";
                }
                foreach (var client in clientsClone)
                {
                    if (client.Key.ToString().Equals(from.ToString()))
                    {
                        if (message == "") message = "Looks like nobody else is here.";
                        NetworkStream stream = client.Value.GetStream();
                        stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                        break;
                    }
                }
            }
            else if (message.ToLowerInvariant().StartsWith("name:"))
            {
                var clientsClone = cloneDictionary(listOfConnectedClients);
                foreach (var client in clientsClone)
                {
                    if (client.Key.ToString().Equals(from.ToString()))
                    {
                        string name = message.Substring("name:".Length);
                        if (name == null)
                        {
                            message = "Name not set.";
                        }
                        else
                        {
                            message = string.Format("Name set successfully to {0}.", name);
                            client.Key.Name = name;
                        }
                        NetworkStream stream = client.Value.GetStream();
                        stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                        break;
                    }
                }
            }
            else
            {
                Brodcast(message, from);
            }
        }

        public static IPAddress GetIpOfClient(TcpClient client)
        {
            return (client.Client.RemoteEndPoint as IPEndPoint).Address;
        }

        //shallow clone
        private static Dictionary<T, U> cloneDictionary<T, U>(Dictionary<T, U> objToClone)
        {
            Dictionary<T, U> cloneOfConnectedClients;
            while (true)
            {
                try
                {
                    cloneOfConnectedClients = new Dictionary<T, U>();
                    foreach (var entry in objToClone)
                    {
                        cloneOfConnectedClients[entry.Key] = entry.Value;
                    }
                }
                catch
                {
                    continue;
                }
                return cloneOfConnectedClients;
            }
        }

        public static IPAddress GetOwnIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }
            return null;
        }
    }
}

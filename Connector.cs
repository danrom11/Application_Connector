using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Application_Connector
{
    public class Connector
    {
        /// <summary>
        /// Write-send data to memory application.
        /// </summary>
        /// <param name="data"> - Data to be transferred.</param>
        /// <param name="memoryName"> - Variable memory department name.</param>
        public static void Send(string data, string memoryName)
        {
            char[] message = data.ToCharArray();
            int size = message.Length;

            // Create a piece of shared memory
            // The first parameter is the name of the site,
            // the second is the length of the memory chunk in bytes: char type takes 2 bytes
            // plus four bytes for one Integer object

            MemoryMappedFile sharedMemory = MemoryMappedFile.CreateOrOpen(memoryName, size * 2 + 4);

            using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size * 2 + 4))
            {
                // write to shared memory
                // write size from zero byte in shared memory
                writer.Write(0, size);
                // write message from the fourth byte in shared memory
                writer.WriteArray<char>(4, message, 0, message.Length);
            }
        }

        /// <summary>
        /// Accept-read data.
        /// </summary>
        /// <param name="memoryName"> - Variable memory department name.</param>
        public static string Accept(string memoryName)
        {
            // Array for message from shared memory
            char[] message;
            // The size of the entered message
            int size;

            // Get an existing piece of shared memory
            // Parameter - site name
            MemoryMappedFile sharedMemory = MemoryMappedFile.OpenExisting(memoryName);
            // First read the size of the message to create an array of the given size
            // Integer takes 4 bytes, starts from the first byte, so we transfer numbers 0 and 4
            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, 4, MemoryMappedFileAccess.Read))
            {
                size = reader.ReadInt32(0);
            }

            // Read the message using the above size
            // The message is a string or array of char objects, each of which is two bytes
            // Therefore, as the second parameter, we pass the number of characters by multiplying by the size in bytes plus
            // And the first parameter is an offset - 4 bytes, which is the size of the message
            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(4, size * 2, MemoryMappedFileAccess.Read))
            {
                //Массив символов сообщения
                message = new char[size];
                reader.ReadArray<char>(0, message, 0, size);
            }
            // Translation to string
            string strMessage = new string(message);
            return strMessage;
        }
    }
    public class MultifunctionalConnector
    {
        // Server
        public MultifunctionalConnector()
        {
            _IPAdrees = IPAddress.Any;
            _PORT = 100;
        }

        public MultifunctionalConnector(string[] Commands)
        {
            _IPAdrees = IPAddress.Any;
            _PORT = 100;
            _Commands = Commands;
        }

        public MultifunctionalConnector(string IPAdress, int PORT)
        {
            _IPAdrees = IPAddress.Parse(IPAdress);
            _PORT = PORT;
        }

        public MultifunctionalConnector(string IPAdress, int PORT, string[] Commands)
        {
            _IPAdrees = IPAddress.Parse(IPAdress);
            _PORT = PORT;
            _Commands = Commands;
        }
               
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private int _PORT;
        private IPAddress _IPAdrees;
        private string[] _Commands;
        private bool sendMsg = false;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public void ServerStart()
        {
            SetupServer();
        }

        public void ServerClose()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        public void ServerSetCommands(string[] Commands)
        {
            _Commands = Commands;
        }

        private void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(_IPAdrees, _PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;
            sendMsg = false;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine(text);
            text = text.Substring(text.IndexOf("->") + 3);

            if (text.Contains("@"))
            {       
                if(StandardCommands(text) == "EX")
                {
                    // Always Shutdown before closing
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                    Console.WriteLine("Client disconnected");
                    return;
                }
                else
                {
                    byte[] data = Encoding.ASCII.GetBytes(StandardCommands(text));
                    current.Send(data);
                }            
            }

                for (int k = 0; k < _Commands.Length; k++)
                {
                    if (text.ToLower() == _Commands[k].ToLower())
                    {
                        sendMsg = true;
                        byte[] data = Encoding.ASCII.GetBytes(_Commands[k + 1]);
                        current.Send(data);
                    }                  
                }
              if(sendMsg != true)
              {
                 byte[] data = Encoding.ASCII.GetBytes("Invalid command");
                 current.Send(data);
              }
            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }

        //List of standard commands
        private string StandardCommands(string command)
        {
            if(command == "@get time")
            {
                sendMsg = true;
                return DateTime.Now.ToLongTimeString();
            }
            if(command == "@get date")
            {
                sendMsg = true;
                return DateTime.Now.ToShortDateString();
            }
            if(command == "@exit")
            {
                sendMsg = true;
                return "EX";
            }
            return "Standard Commands -> The command is missing";
        }




        // Client
        private static readonly Socket ClientSocket = new Socket
           (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void ClientConnect()
        {
            if(_IPAdrees == IPAddress.Any)
            {
                _IPAdrees = IPAddress.Loopback;
            }
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    //Console.WriteLine("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(_IPAdrees, _PORT);
                }
                catch (SocketException)
                {
                   // Console.Clear();
                }
            }

            // Console.Clear();
            //Console.WriteLine("Connected");
        }

        public string Request(string text)
        {
                SendString(text, _IPAdrees);
                return ReceiveResponse();          
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        public void ClientDisconnect()
        {
            SendString("@exit", _IPAdrees);
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            //Environment.Exit(0);
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private static void SendString(string text, IPAddress ipClient)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(Convert.ToString(ipClient) + " -> " + text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static string ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return null;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            return text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;

namespace ConsoleCOMSERVER
{
    class Program
    {
        static IPAddress ipAddress = IPAddress.Any;
        static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 15195);

        static Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        static List<Socket> handler = new List<Socket>();

        //static Dictionary<int, Socket> handler = new Dictionary<int, Socket>();

        static int id = 0;
        static void Main(string[] args)
        {
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");

                while (true)
                {
                    handler.Add(listener.Accept());
                    id++;

                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        int clientid = handler.Count;
                        clientid--;
                        Client(handler[clientid]);
                    }).Start();
                    Console.WriteLine("user connected.");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
        public static void Client(Socket socket)
        {
            string data = null;
            byte[] bytes = new Byte[1024];
            try
            {
                while (true)
                {
                    if (SocketConnected(socket))
                    {
                        data = null;
                        bytes = new byte[1024];
                        int bytesRec = socket.Receive(bytes);
                        data += Encoding.Unicode.GetString(bytes, 0, bytesRec);

                        if (data != null)
                        {
                            Console.WriteLine(data);

                            byte[] msg = Encoding.Unicode.GetBytes(data);
                            for (int i = 0; i < handler.Count; i++)
                            {
                                if (SocketConnected(handler[i]))
                                {
                                    //if (handler[i] != socket)
                                    handler[i].Send(msg);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                socket.Close();
                Console.WriteLine("user disconnect: " + e);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static bool SocketConnected(Socket s)
        {
            if (s.Connected == true)
            {
                bool part1 = s.Poll(1000, SelectMode.SelectRead);
                bool part2 = (s.Available == 0);
                if (part1 && part2)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
    }
}

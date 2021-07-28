using System;
using System.Net;
using System.Net.Sockets;
using ServerCore;

namespace GameServer
{

    class Program
    {
        static Listener _listener = new Listener();
        public static int Port { get; } = 7777;
        public static string IpAddress { get; set; } 


        static void Main(string[] args)
        {
            //DNS [ domain name system]

            string name = Dns.GetHostName();
            Console.WriteLine(name);
            IPHostEntry iphost = Dns.GetHostEntry(name);
            IPAddress ipAddr = iphost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);
            Console.WriteLine(endPoint);

            IpAddress = ipAddr.ToString();

            // Listner 등록 
        //    _listener.Init(endPoint); 




        }
    }
}

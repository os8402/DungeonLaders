using ServerCore;
using System;
using System.Net;

namespace DummyClient
{
    class Program
    {
        static Connector connector = new Connector();
        static int port = 7777;

        static int dummyCount = 5;

        static void Main(string[] args)
        {
            string name = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(name);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

          //  connector.Init(endPoint, { ()=> return new Session()}, dummyCount);

            //IPEndPoint
        }
    }
}

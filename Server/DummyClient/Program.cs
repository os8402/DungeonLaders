using DummyClient.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        static int DummyClientCount { get; } = 20;
        static void Main(string[] args)
        {
            Thread.Sleep(3000);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];


            List<int> portList = new List<int>() { 6666, 7777, 8888 };
            List<int> connectList = new List<int>() { 90, 90, 40  };
            int idx = 0;

            foreach(int port in portList)
            {

  
                IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

                Connector connector = new Connector();

                connector.Connect(endPoint,
                    () => { return SessionManager.Instance.Generate(); },
                   connectList[idx++]);

            }

       
            while(true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}

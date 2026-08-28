using DummyClient.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        static int DummyClientCount { get; } = 20;
        /// <summary>GameServer.Program.GetLocalIPAddress 와 동일한 로직.</summary>
        static IPAddress GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect("8.8.8.8", 65530);
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                        return endPoint.Address;
                }
            }
            catch (SocketException) { }

            IPAddress fallback = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork
                                     && !IPAddress.IsLoopback(a)
                                     && !a.ToString().StartsWith("169.254."));

            return fallback ?? IPAddress.Loopback;
        }

        static void Main(string[] args)
        {
            Thread.Sleep(3000);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            // GameServer 와 동일한 이유로 수정: AddressList[1] 은 IPv6 링크로컬이나
            // 가상 어댑터(WSL/Hyper-V)를 잡는다. 라우팅 테이블에 직접 물어본다.
            IPAddress ipAddr = GetLocalIPAddress();


            List<int> portList = new List<int>() { 6666, 7777, 8888 };
            List<int> connectList = new List<int>() { 90, 100, 40  };
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

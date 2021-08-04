﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;


namespace GameServer
{


	class Program
	{
		static Listener _listener = new Listener();
	

		static void FlushRoom()
        {
			JobTimer.Instace.Push(FlushRoom, 250);
        }

		static void Main(string[] args)
        {
           

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			FlushRoom();

			while (true)
			{
				JobTimer.Instace.Flush();
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Data;
using GameServer.DB;
using GameServer.Game;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using SharedDB;

namespace GameServer
{
    //1.GameRoom 방식
    //2.더 넓은 영역 관리
    //3. 심리스 MMO


    //사용 중인 스레드
    //1. Recv(N개)
    //2. GameLogic(1)
    //3. Send(1)
    //4. DbTask(1)

	class Program
	{
		static Listener _listener = new Listener();

        static void GameLogicTask()
        {
            while(true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }
        static void DbTask()
        {
            while (true)
            {
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }
        static void NetworkTask()
        {
            while(true)
            {
                List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach(ClientSession session in sessions)
                {
                    session.FlushSend();
                }

                Thread.Sleep(0);
            }
        }

        static void StartServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += ((s, e) =>
            {

                using (SharedDbContext shared = new SharedDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == ServerName).FirstOrDefault();
                    if (serverDb != null)
                    {
                        serverDb.IpAddress = IpAddress;
                        serverDb.Port = Port;
                        serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = ServerName,
                            IpAddress = Program.IpAddress,
                            Port = Port, 
                            BusyScore = SessionManager.Instance.GetBusyScore()
                        };
                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();

                    }
                }
      
            });
            t.Interval = 10 * 1000;
            t.Start();
        }

        public static string ServerName { get; set; }
        public static int Port { get; set; }
        public static string IpAddress { get; set; }


        static void Main(string[] args)
        {
    
			ConfigManager.LoadConfig();
			DataManager.LoadData();

            GameLogic.Instance.Push(() => { GameRoom room = GameLogic.Instance.Add(1); });

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];


            ServerName = ConfigManager.Config.serverList.name;
            Port = ConfigManager.Config.serverList.port;
            IpAddress = ipAddr.ToString();


            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);      
             _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

             StartServerInfoTask();
            

            Console.WriteLine("Listening...");

            //DB Task
            {
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
            }
            //NetworkSend Task
            {
                Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }


            //GameLogic
            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();

        }
	}
}
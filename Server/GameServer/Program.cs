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


        //static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

        //static void TickRoom(GameRoom room, int tick = 100)
        //{
        //    var timer = new System.Timers.Timer();
        //    timer.Interval = tick;
        //    timer.Elapsed += ((s, e) => { room.Update(); });
        //    timer.AutoReset = true;
        //    timer.Enabled = true;

        //    _timers.Add(timer);
        //}

        static void Main(string[] args)
        {
			ConfigManager.LoadConfig();
			DataManager.LoadData();

            GameLogic.Instance.Push(() => { GameRoom room = GameLogic.Instance.Add(1); });
  
          //  TickRoom(room, 50);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
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
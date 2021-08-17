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


	class Program
	{
		static Listener _listener = new Listener();
        static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();
        
        static void TickRoom(GameRoom room, int tick = 100)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { room.Update(); });
            timer.AutoReset = true;
            timer.Enabled = true;

            _timers.Add(timer);
        }

		static void Main(string[] args)
        {
			ConfigManager.LoadConfig();
			DataManager.LoadData();
            //test
            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb player = db.Players.FirstOrDefault();
                if(player != null)
                {
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 201,
                        Count = 1,
                        Slot = 0,
                        Owner = player
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 1001,
                        Count = 1,
                        Slot = 1,
                        Owner = player
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 1101,
                        Count = 1,
                        Slot = 2,
                        Owner = player
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 2001,
                        Count = 10,
                        Slot = 5,
                        Owner = player
                    });

                    db.SaveChangesEx();
                }
            }

      
			GameRoom room =  RoomManager.Instance.Add(1);
            TickRoom(room, 50);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            while (true)
            {
                DbTransaction.Instance.Flush();
            }

        }
	}
}
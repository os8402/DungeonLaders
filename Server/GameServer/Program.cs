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

        /// <summary>서버 목록 갱신 주기. AccountServer 의 판정 기준과 짝을 이룬다.</summary>
        const int HeartbeatIntervalMs = 10 * 1000;

        static void StartServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += (s, e) => WriteServerInfo();
            t.Interval = HeartbeatIntervalMs;
            t.Start();

            // Timer 는 Interval 이 지난 뒤에야 처음 발동한다.
            // 그 사이(최대 10초)에 로그인하면 서버 목록이 비어 보이므로 기동 직후 한 번 즉시 기록한다.
            WriteServerInfo();
        }

        /// <summary>
        /// 이 서버의 접속 정보와 혼잡도를 SharedDB 에 기록한다(upsert).
        /// LastPingTime 을 함께 갱신해 "살아있음"을 알린다 — AccountServer 는 이 값으로
        /// 죽은 서버를 목록에서 걸러낸다.
        /// </summary>
        static void WriteServerInfo()
        {
            try
            {
                using (SharedDbContext shared = new SharedDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == ServerName).FirstOrDefault();
                    if (serverDb != null)
                    {
                        serverDb.IpAddress = IpAddress;
                        serverDb.Port = Port;
                        serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
                        serverDb.LastPingTime = DateTime.UtcNow;
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = ServerName,
                            IpAddress = IpAddress,
                            Port = Port,
                            BusyScore = SessionManager.Instance.GetBusyScore(),
                            LastPingTime = DateTime.UtcNow
                        };
                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();
                    }
                }
            }
            catch (Exception e)
            {
                // System.Timers.Timer 는 콜백에서 터진 예외를 조용히 삼킨다.
                // 그래서 원래는 DB 가 없어도 서버가 멀쩡히 떠 있는 것처럼 보였다. 반드시 남긴다.
                Console.WriteLine($"[ServerInfo] SharedDB 갱신 실패 : {e.Message}");
            }
        }

        public static string ServerName { get; set; }
        public static int Port { get; set; }
        public static string IpAddress { get; set; }



        /// <summary>
        /// 클라이언트가 접속할 수 있는 이 PC 의 IPv4 주소를 고른다.
        ///
        /// 원본은 <c>Dns.GetHostEntry(host).AddressList[1]</c> 를 그대로 썼다.
        /// 2021년 개발 PC 에서는 우연히 맞았지만 지금은 거의 항상 틀린다 —
        /// 실제로 이 코드를 되살린 PC 의 AddressList 는 다음과 같았다.
        ///
        ///   [0] fe80::832c:...   IPv6 링크로컬
        ///   [1] fe80::ce9:...    IPv6 링크로컬   ← 원본이 고르던 값
        ///   [2] fe80::4c3f:...   IPv6 링크로컬
        ///   [3] 172.23.64.1      Hyper-V 가상 스위치
        ///   [4] 192.168.219.105  실제 Wi-Fi        ← 이게 정답
        ///   [5] 172.18.112.1     에뮬레이터 가상 어댑터
        ///
        /// 단순히 "첫 번째 IPv4" 를 골라도 가상 어댑터(172.23.64.1)에 걸린다.
        /// 그래서 라우팅 테이블에 직접 물어본다 — UDP 소켓을 외부 주소로 Connect 하면
        /// (UDP 는 실제 패킷을 보내지 않는다) OS 가 어떤 인터페이스를 쓸지 결정하고,
        /// LocalEndPoint 에 그 인터페이스의 주소가 채워진다.
        /// </summary>
        static IPAddress GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    // 8.8.8.8 로 실제 통신하지는 않는다. 경로 결정에만 쓴다.
                    socket.Connect("8.8.8.8", 65530);
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                        return endPoint.Address;
                }
            }
            catch (SocketException)
            {
                // 네트워크가 아예 없는 환경 (오프라인 CI 등) — 아래 폴백으로 넘어간다.
            }

            // 폴백: IPv4 중 링크로컬(169.254.x.x)과 루프백을 뺀 첫 번째
            IPAddress fallback = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork
                                     && !IPAddress.IsLoopback(a)
                                     && !a.ToString().StartsWith("169.254."));

            return fallback ?? IPAddress.Loopback;
        }

        static void Main(string[] args)
        {
    
			ConfigManager.LoadConfig();
			DataManager.LoadData();

            GameLogic.Instance.Push(() => { GameRoom room = GameLogic.Instance.Add(1); });

            IPAddress ipAddr = GetLocalIPAddress();


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
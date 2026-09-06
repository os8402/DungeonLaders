using DummyClient.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DummyClient
{
    /// <summary>AccountServer 로그인으로 받은 게임 서버 입장 자격. DummyId 순서로 세션에 배정된다.</summary>
    public class LoginInfo
    {
        public int AccountDbId;
        public int Token;
    }

    /// <summary>
    /// [2026-09 추가] 부하 측정용 집계. 서버 코드는 건드리지 않고 DummyClient 쪽에서만 왕복 시간을 잰다.
    ///   LoginRtt : C_Login 송신 → S_Login 수신     (Recv 스레드에서 SharedDB 토큰 조회 + 게임 DB 계정 조회)
    ///   EnterRtt : C_EnterGame 송신 → S_EnterGame 수신 (Recv 스레드 → GameLogic 잡 큐 → 100ms 주기 Send flush)
    /// 서버의 S_Ping 은 주석 처리되어 있어(ClientSession.OnConnected) 핑/퐁 RTT 는 잴 수 없다.
    /// </summary>
    public static class LoadStats
    {
        static object _lock = new object();
        static int _loginOk, _loginFail, _inGame, _disconnected, _respawn;
        static long _loginSum, _loginMax, _enterSum, _enterMax;
        // 백분위(p50/p95) 계산용 원본 샘플. 세션 수백 개 규모라 그냥 다 들고 있는다.
        static List<long> _loginSamples = new List<long>(), _enterSamples = new List<long>();

        public static void RecordLogin(long ms) { lock (_lock) { _loginOk++; _loginSum += ms; if (ms > _loginMax) _loginMax = ms; _loginSamples.Add(ms); } }
        public static void RecordEnter(long ms) { lock (_lock) { _inGame++; _enterSum += ms; if (ms > _enterMax) _enterMax = ms; _enterSamples.Add(ms); } }
        public static void RecordLoginFail() { lock (_lock) { _loginFail++; } }
        public static void RecordDisconnect() { lock (_lock) { _disconnected++; } }
        public static void RecordRespawn() { lock (_lock) { _respawn++; } }

        public static string Summary()
        {
            lock (_lock)
            {
                string login = _loginOk > 0 ? $"{_loginSum / _loginOk}/{Percentile(_loginSamples, 50)}/{Percentile(_loginSamples, 95)}/{_loginMax}" : "-";
                string enter = _inGame > 0 ? $"{_enterSum / _inGame}/{Percentile(_enterSamples, 50)}/{Percentile(_enterSamples, 95)}/{_enterMax}" : "-";
                return $"connected={Session.SessionManager.Instance.Count} loginOk={_loginOk} loginFail={_loginFail} inGame={_inGame} respawn={_respawn} disconnected={_disconnected} "
                     + $"loginRtt(avg/p50/p95/max ms)={login} enterRtt(avg/p50/p95/max ms)={enter}";
            }
        }

        // nearest-rank 백분위. 호출자가 _lock 을 잡고 있다.
        static long Percentile(List<long> samples, int p)
        {
            if (samples.Count == 0) return 0;
            List<long> sorted = samples.OrderBy(v => v).ToList();
            int rank = (int)Math.Ceiling(p / 100.0 * sorted.Count) - 1;
            return sorted[Math.Clamp(rank, 0, sorted.Count - 1)];
        }
    }

    /// <summary>
    /// 더미 계정을 AccountServer 에 만들고 로그인해서 토큰을 받는다.
    /// 게임 서버가 토큰을 검증하게 되면서 더미도 정식 경로(계정 서버 → 토큰 → 게임 서버)를 타야 한다.
    /// </summary>
    static class AccountApi
    {
        class LoginRes
        {
            public bool LoginOk { get; set; }
            public int AccountDbId { get; set; }
            public int Token { get; set; }
        }

        // dotnet dev-certs 의 자체 서명 인증서를 그대로 받아들인다. 로컬 테스트 도구라서 허용.
        static readonly HttpClient _http = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        /// <summary>dummy0001.. 계정을 만들고(이미 있으면 무시) 로그인한다. 10개씩 병렬.</summary>
        public static async Task<LoginInfo[]> LoginAllAsync(string baseUrl, int count)
        {
            LoginInfo[] result = new LoginInfo[count];
            for (int start = 0; start < count; start += 10)
            {
                int end = Math.Min(start + 10, count);
                Task[] batch = new Task[end - start];
                for (int i = start; i < end; i++)
                {
                    int id = i + 1;
                    batch[i - start] = Task.Run(async () => result[id - 1] = await LoginOneAsync(baseUrl, $"dummy{id:0000}", "dummy"));
                }
                await Task.WhenAll(batch);
                Console.WriteLine($"AccountServer 로그인 {end}/{count}");
            }
            return result;
        }

        static async Task<LoginInfo> LoginOneAsync(string baseUrl, string name, string password)
        {
            try
            {
                var body = new { AccountName = name, Password = password };
                // 이미 있는 계정이면 CreateOk=false 가 돌아올 뿐이다.
                await _http.PostAsJsonAsync($"{baseUrl}/api/account/create", body);

                HttpResponseMessage resp = await _http.PostAsJsonAsync($"{baseUrl}/api/account/login", body);
                LoginRes res = await resp.Content.ReadFromJsonAsync<LoginRes>();
                if (res == null || res.LoginOk == false)
                    return null;

                return new LoginInfo { AccountDbId = res.AccountDbId, Token = res.Token };
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AccountServer] {name} 로그인 실패: {e.Message}");
                return null;
            }
        }
    }

    class Program
    {
        /// <summary>DummyId(1부터) - 1 번째 항목이 그 세션의 입장 자격이다.</summary>
        public static LoginInfo[] LoginInfos { get; private set; } = new LoginInfo[0];
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

        // 사용법: DummyClient [--bogus] [--account-server https://localhost:5001]
        //   --bogus : AccountServer 를 거치지 않고 가짜 (AccountDbId, Token) 으로 로그인한다.
        //             게임 서버가 전부 LoginOk=0 으로 거부하고 끊어야 정상이다 (docs/AUTH_FIX.md 검증용).
        static void Main(string[] args)
        {
            bool bogus = Array.IndexOf(args, "--bogus") >= 0;
            string accountServer = "https://localhost:5001";
            int optIdx = Array.IndexOf(args, "--account-server");
            if (optIdx >= 0 && optIdx + 1 < args.Length)
                accountServer = args[optIdx + 1];

            List<int> portList = new List<int>() { 6666, 7777, 8888 };
            List<int> connectList = new List<int>() { 90, 100, 40  };
            int total = connectList.Sum();

            if (bogus)
            {
                LoginInfos = new LoginInfo[total];
                for (int i = 0; i < total; i++)
                    LoginInfos[i] = new LoginInfo { AccountDbId = 900000 + i, Token = 1 };
                Console.WriteLine($"[bogus] 가짜 토큰 {total}개로 접속 — 게임 서버가 전부 거부해야 정상");
            }
            else
            {
                LoginInfos = AccountApi.LoginAllAsync(accountServer, total).Result;
                int ok = LoginInfos.Count(l => l != null);
                if (ok < total)
                {
                    Console.WriteLine($"AccountServer 로그인 {ok}/{total} 성공 — 부족합니다. {accountServer} 에 AccountServer 가 떠 있는지 확인하세요.");
                    return;
                }
            }

            Thread.Sleep(3000);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            // GameServer 와 동일한 이유로 수정: AddressList[1] 은 IPv6 링크로컬이나
            // 가상 어댑터(WSL/Hyper-V)를 잡는다. 라우팅 테이블에 직접 물어본다.
            IPAddress ipAddr = GetLocalIPAddress();


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
                Console.WriteLine($"[stats] {LoadStats.Summary()}");
            }
        }
    }
}

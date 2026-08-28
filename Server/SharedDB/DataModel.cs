using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedDB
{
    [Table("Token")]
    public class TokenDb
    {
        public int TokenDbId { get; set; }
        public int AccountDbId { get; set; }
        public int Token { get; set; }
        public DateTime Expired { get; set; }

    }

    [Table("ServerInfo")]
    public class ServerDb
    {
        public int ServerDbId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int BusyScore { get; set; }

        /// <summary>
        /// 게임 서버가 마지막으로 자기 상태를 갱신한 시각 (UTC).
        ///
        /// 원래는 이 값이 없어서, 한 번 켰던 서버가 종료되어도 ServerInfo 행이
        /// 그대로 남았다. 클라이언트는 죽은 서버를 목록에서 보고 접속을 시도하다
        /// ConnectionRefused 를 맞았다. 이 값으로 살아있는 서버만 걸러낸다.
        /// </summary>
        public DateTime LastPingTime { get; set; }
    }
}

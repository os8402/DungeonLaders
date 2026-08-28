using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
        SharedDbContext _shared;

        /// <summary>이 시간 동안 갱신이 없는 게임 서버는 목록에서 제외한다 (하트비트 주기 10초의 3배).</summary>
        const int ServerAliveTimeoutSeconds = 30;

        public AccountController(AppDbContext context , SharedDbContext shared)
        {
            _context = context;
            _shared = shared; 
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRes res = new
                CreateAccountPacketRes();

            // 원래는 검증이 없어서 이름·비밀번호가 빈 문자열인 계정이 그대로 DB 에 들어갔다.
            if (string.IsNullOrWhiteSpace(req?.AccountName) || string.IsNullOrWhiteSpace(req?.Password))
            {
                res.CreateOk = false;
                return res;
            }

            AccountDb account =  _context.Accounts
                           .AsNoTracking()
                          .Where(a => a.AccountName == req.AccountName)
                          .FirstOrDefault();


            if(account == null)
            {
                _context.Accounts.Add(new AccountDb()
                { 
                    AccountName = req.AccountName,
                    Password = req.Password,
                });

                bool success = _context.SaveChangesEx();
                res.CreateOk = success;

            }
            else
            {
                res.CreateOk = false; 
            }


            return res; 

        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            if (string.IsNullOrWhiteSpace(req?.AccountName) || string.IsNullOrWhiteSpace(req?.Password))
            {
                res.LoginOk = false;
                return res;
            }

            AccountDb account = _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                .FirstOrDefault();


            if ( account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;

                //토큰 발급
                DateTime expired = DateTime.UtcNow;
                expired.AddSeconds(600);

                TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
                if(tokenDb != null)
                {
                    tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
                    tokenDb.Expired = expired;
                    _shared.SaveChangesEx();
                }
                else
                {
                    tokenDb = new TokenDb()
                    {
                        AccountDbId = account.AccountDbId,
                        Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                        Expired = expired
                    };
                    _shared.Tokens.Add(tokenDb);
                    _shared.SaveChangesEx();
                }

                res.AccountDbId = account.AccountDbId;
                res.Token = tokenDb.Token;
                res.ServerList = new List<ServerInfo>();

                // 게임 서버는 10초마다 LastPingTime 을 갱신한다.
                // 그 3배(30초) 안에 소식이 없으면 죽은 것으로 보고 목록에서 뺀다.
                // 이게 없던 원래 코드는 한 번 켰다 끈 서버가 목록에 영원히 남아,
                // 클라이언트가 접속을 시도하다 ConnectionRefused 를 맞았다.
                DateTime aliveSince = DateTime.UtcNow.AddSeconds(-ServerAliveTimeoutSeconds);

                foreach(ServerDb serverDb in _shared.Servers.Where(s => s.LastPingTime >= aliveSince))
                {
                    res.ServerList.Add(new ServerInfo()
                    {
                        Name = serverDb.Name,
                        IpAddress = serverDb.IpAddress,
                        Port = serverDb.Port,
                        BusyScore = serverDb.BusyScore
                    });

                }

            }

            return res; 
        }
    }
}

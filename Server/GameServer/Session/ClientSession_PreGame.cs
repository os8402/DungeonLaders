using GameServer.Data;
using GameServer.DB;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using ServerCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        /// <summary>
        /// AccountServer 가 로그인 시 SharedDB.Token 에 기록한 (AccountDbId, Token) 쌍과 대조한다.
        ///
        /// 원래는 이 검증이 없었다. 클라이언트가 보낸 UniqueId(설치 경로 해시)를 그대로 계정 이름으로 썼고
        /// 없으면 새로 만들었다. 즉 게임 서버 포트에 직접 붙어 아무 문자열이나 보내면 비밀번호 없이
        /// 그 계정으로 들어갈 수 있었고, AccountServer 의 로그인·토큰 발급은 게임 진입에 아무 영향이 없었다.
        ///
        /// DB 를 읽지 못한 경우도 실패로 본다(fail-closed). 예외를 Recv 스레드로 흘리면
        /// Session.OnRecvCompleted 가 잡기는 하지만 RegisterRecv 가 다시 걸리지 않아
        /// 세션이 "붙어는 있는데 아무것도 못 받는" 상태로 남는다 (docs/LOADTEST.md 관찰 참고).
        /// </summary>
        bool VerifyToken(int accountDbId, int token)
        {
            if (accountDbId <= 0)
                return false;

            try
            {
                using (SharedDbContext shared = new SharedDbContext())
                {
                    TokenDb tokenDb = shared.Tokens
                        .AsNoTracking()
                        .Where(t => t.AccountDbId == accountDbId)
                        .FirstOrDefault();

                    if (tokenDb == null || tokenDb.Token != token)
                        return false;

                    // AccountServer 가 발급 시각 + 600초로 기록한다 (만료 계산 버그도 같이 고쳤다)
                    if (tokenDb.Expired < DateTime.UtcNow)
                        return false;

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Login] SharedDB 토큰 조회 실패 : {e.Message}");
                return false;
            }
        }

        public void HandleLogin(C_Login loginPacket)
        {
            //보안 체크~ 
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            // 토큰 검증 — 실패하면 LoginOk=0 을 보내고 끊는다.
            // Send 는 예약만 하고 Network 스레드가 최대 100ms 뒤에 flush 하므로, 바로 Disconnect 하면 패킷이 안 나간다.
            if (VerifyToken(loginPacket.AccountDbId, loginPacket.Token) == false)
            {
                Console.WriteLine($"[Login] 토큰 검증 실패 AccountDbId={loginPacket.AccountDbId} → 접속 종료");
                Send(new S_Login() { LoginOk = 0 });
                GameLogic.Instance.PushAfter(1000, Disconnect);
                return;
            }

            // 게임 DB(DungeonLadersDB.Account)의 계정은 AccountServer 의 AccountDbId 로 찾는다.
            // (이전: 클라이언트가 보낸 설치 경로 해시. 컬럼을 추가하지 않고 기존 AccountName 에 ID 문자열을 넣는다)
            string accountName = loginPacket.AccountDbId.ToString();

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == accountName).FirstOrDefault();

                if (findAccount != null)
                {
                    //AccountDbID 메모리에 보존
                    AccountDbId = findAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };

                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name = playerDb.PlayerName,
                            StatInfo = new StatInfo
                            {
                                Level = playerDb.Level,
                                Hp = playerDb.Hp,
                                MaxHp = playerDb.MaxHp,
                                Mp = playerDb.Mp,
                                MaxMp = playerDb.MaxMp,
                                Attack = playerDb.Attack,
                                Speed = playerDb.Speed,
                                CurExp = playerDb.CurExp,
                                TotalExp = playerDb.TotalExp
                            },
  
                        };


                        List<ItemDb> itemList = db.Items
                            .AsNoTracking()
                            .Where(i => i.OwnerDbId == playerDb.PlayerDbId)
                            .ToList();

                        
                        if(itemList != null)
                        {
                            foreach (ItemDb itemDb in itemList)
                            {
                                if (itemDb.Equipped)
                                    lobbyPlayer.EquippedItemList.Add(itemDb.TemplateId);
                            }
                        }

        
                        //메모리에 보존
                        LobbyPlayers.Add(lobbyPlayer);

                        //패킷에 넣기
                        loginOk.Players.Add(lobbyPlayer);
                    }

                    Send(loginOk);
                    //로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    AccountDb newAccount = new AccountDb() { AccountName = accountName };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;


                    //AccountDbID 메모리에 보존
                    AccountDbId = newAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    Send(loginOk);
                    //로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if (playerInfo == null)
                return;

            MyPlayer = ObjectManager.Instance.Add<Player>();

            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.PosInfo.State = ControllerState.Idle;
                MyPlayer.PosInfo.Target.Dir = DirState.Left;
                MyPlayer.Info.TeamId = SessionId << 24;
               
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;

                S_ItemList itemListPacket = new S_ItemList();

                //아이템 목록을 갖고온다. 
                using (AppDbContext db = new AppDbContext())
                {
                    List<ItemDb> items = db.Items
                        .Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
                        .ToList();

                    foreach (ItemDb itemDb in items)
                    {
                        Item item = Item.MakeItem(itemDb);
                        if (item != null)
                        {
                            MyPlayer.Inven.Add(item);
                            if(item.ItemType == ItemType.Weapon)
                            {
                                if (item.Equipped)
                                {
                                    MyPlayer.EquipWeapon =
                                       ObjectManager.Instance.CreateObjectWeapon(item.TemplateId);
                                }

                            }
                            ItemInfo info = new ItemInfo();
                            info.MergeFrom(item.Info);
                            itemListPacket.Items.Add(info);
                        }

                    }

                }
                //TODO 클라한테 아이템 목록 전달
                Send(itemListPacket);


            }

            ServerState = PlayerServerState.ServerStateGame;

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.EnterGame, MyPlayer, true);
            });

            //TODO 입장 요청


        }

        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            //보안 체크~ 
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb findPlayer = db.Players
                     .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

                if (findPlayer != null)
                {
                    //이름이 겹쳤다!
                    Send(new S_CreatePlayer());
                }
                else
                {

                    //1레벨 스탯 정보 추출
                    StatInfo stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);

                    //만들 때 한번에 // 전사 // 궁수 // 마법사 생성
                    //별도로 create 만들기가 귀찮

                    S_CreatePlayer newPlayer = new S_CreatePlayer();

                    string[] jobs = { "MyWarrior_", "MyArcher_", "MyMage_" };
                    int[] startWeapons = { 101, 301, 401 };

                    for (int i = 0; i < 3; i++)
                    {
                        LobbyPlayerInfo createPlayer = CreatePlayerAll(jobs[i], stat, db, createPacket);

                        if (createPlayer == null)
                            return;

                        //장착한 무기도 보냄
                        createPlayer.EquippedItemList.Add(startWeapons[i]);

                        newPlayer.Players.Add(createPlayer);

                        ItemDb newItemDb = new ItemDb()
                        {
                            TemplateId = startWeapons[i],
                            Count = 1,
                            Slot = 0,
                            OwnerDbId = createPlayer.PlayerDbId,
                            Equipped = true
                        };

                        db.Items.Add(newItemDb);

                        bool success = db.SaveChangesEx();
                        if (success == false)
                            return;

                    }



                    Send(newPlayer);
                }
            }

        }

        LobbyPlayerInfo CreatePlayerAll(string jobName , StatInfo stat, AppDbContext db, C_CreatePlayer createPacket)
        {


            //Db에 플레이어 만듬
            PlayerDb newPlayerDb = new PlayerDb()
            {
                PlayerName = jobName + createPacket.Name,
                Level = stat.Level,
                Hp = stat.Hp,
                MaxHp = stat.MaxHp,
                Mp = stat.Mp,
                MaxMp = stat.MaxMp,
                Attack = stat.Attack,
                Speed = stat.Speed,
                CurExp = 0,
                TotalExp = stat.TotalExp,
                AccountDbId = AccountDbId
            };

            db.Players.Add(newPlayerDb);

            bool success = db.SaveChangesEx();
            if (success == false)
                return null;


            //메모리에 추가
            LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
            {
                PlayerDbId = newPlayerDb.PlayerDbId,
                Name = newPlayerDb.PlayerName,
                StatInfo = new StatInfo()
                {
                    Level = stat.Level,
                    Hp = stat.Hp,
                    MaxHp = stat.MaxHp,
                    Mp = stat.Mp,
                    MaxMp = stat.MaxMp,
                    Attack = stat.Attack,
                    Speed = stat.Speed,
                    CurExp = 0,
                    TotalExp = stat.TotalExp,

                }

            };

            //메모리에 등록
            LobbyPlayers.Add(lobbyPlayer);
            return lobbyPlayer;
        }


    }
}

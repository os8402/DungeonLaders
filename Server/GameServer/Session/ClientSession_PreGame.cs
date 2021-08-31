using GameServer.Data;
using GameServer.DB;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using ServerCore;
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

        public void HandleLogin(C_Login loginPacket)
        {
            //보안 체크~ 
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

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
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
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
                MyPlayer.Info.TeamId = 1 << 24;
               
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

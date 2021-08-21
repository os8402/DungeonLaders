using GameServer.Data;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        //ME [Gameroom] -> You [Db] -> Me [GameRoom]
        public static void SavePlayerStatus_Hp(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            //ME [Gameroom]
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            // you [DB]

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //ME  
                        room.Push(() =>
                        {
                         //   Console.WriteLine($"HpSaved_{playerDb.Hp}");
                        });
                    }

                }
            });


        }
        public static void SavePlayerStatus_Exp(Player player , GameRoom room)
        {
            if (player == null || room == null)
                return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.CurExp = player.Exp;

            Instance.Push(()=>
            {
                using(AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.CurExp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if(success)
                    {

                    }
                }
            });
        }

        public static void SavePlayerStatus_All(Player player , GameRoom room)
        {
            if (player == null || room == null)
                return;

    
            //메모리에 추가
            LobbyPlayerInfo playerInfo =
                player.Session.LobbyPlayers.Find((l) => l.Name == player.Info.Name);
            
            playerInfo.StatInfo.MergeFrom(player.Stat);
           

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Level = player.Stat.Level;
            playerDb.Hp = player.Stat.MaxHp;
            playerDb.MaxHp = player.Stat.MaxHp;
            playerDb.Mp = player.Stat.MaxMp;
            playerDb.MaxMp = player.Stat.MaxMp;
            playerDb.Attack = player.Stat.Attack;
            playerDb.Speed = player.Stat.Speed;
            playerDb.CurExp = 0;
            playerDb.TotalExp = player.Stat.TotalExp;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.Level)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.MaxHp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.Mp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.MaxMp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.Attack)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.Speed)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.CurExp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.TotalExp)).IsModified = true;
                    bool success = db.SaveChangesEx();

                    if(success)
                    {
                        room.Push(() =>
                        {
                            //클라에 레벨업 패킷 전송
  
                            S_LevelUp upPacket = new S_LevelUp();
                        
                            StatInfo statInfo = new StatInfo();
                            statInfo.MergeFrom(player.Stat);

                            upPacket.ObjectId = player.Id;
                            upPacket.StatInfo = new StatInfo();
                            upPacket.StatInfo.MergeFrom(statInfo);

                            player.Session.Send(upPacket);
                        
                        });

                    }

                }
            });

        }
        

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId,
            };

            // you [DB]

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                 
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //ME  
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);

                            //클라 처리 
                            S_AddItem itemPacket = new S_AddItem();
                            ItemInfo itemInfo = new ItemInfo();
                            itemInfo.MergeFrom(newItem.Info);
                            itemPacket.Items.Add(itemInfo);

                            player.Session.Send(itemPacket);

                        });
                    }

                }
            });


        }

    }
}

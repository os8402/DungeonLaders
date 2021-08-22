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

        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            //int? slot = player.Inven.GetEmptySlot();
            //if (slot == null)
            //    return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped

            };

            // you [DB]

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                       //실패 시 처리
                    }

                }
            });

        }
    }

}

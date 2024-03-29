﻿using GameServer.Data;
using GameServer.DB;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            player.HandleEquipItem(equipPacket);
        }
        public void HandleUseItem(Player player, C_UseItem usePacket)
        {
            if (player == null)
                return;

            player.HandleUseItem(usePacket);
        }

        public void HandleRemoveItem(Player player , C_RemoveItem removePacket)
        {
            if (player == null)
                return;

            player.HandleRemoveItem(removePacket);
        }
    }
}

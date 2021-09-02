using GameServer;
using GameServer.DB;
using GameServer.Game;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		// Console.WriteLine($"C_Move({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleMove, player, movePacket);

	}

	public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;


		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleSkill, player, skillPacket);

	}
	public static void C_LoginHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = packet as C_Login;
		ClientSession clientSession = session as ClientSession;
		clientSession.HandleLogin(loginPacket);

	}
	public static void C_EnterGameHandler(PacketSession session, IMessage packet)
	{
		C_EnterGame enterGamePacket = (C_EnterGame)packet;
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandleEnterGame(enterGamePacket);
	}
	public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		C_CreatePlayer createPlayerPacket = (C_CreatePlayer)packet;
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandleCreatePlayer(createPlayerPacket);
	}
	public static void C_EquipItemHandler(PacketSession session, IMessage packet)
	{
		C_EquipItem equipPacket = (C_EquipItem)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleEquipItem, player, equipPacket);


	}
	public static void C_PongHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandlePong();
	}
    public static void C_LevelUpHandler(PacketSession session, IMessage packet)
    {
		C_LevelUp upPacket = (C_LevelUp)packet;
		ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

		room.Push(room.HandleLevelUp, player, upPacket);
	}
    public static void C_UseItemHandler(PacketSession session, IMessage packet)
    {
		C_UseItem usePacket = (C_UseItem)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleUseItem, player, usePacket);


    }
	
	
    public static void C_RemoveItemHandler(PacketSession session, IMessage packet)
    {
		C_RemoveItem removePacket = (C_RemoveItem)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleRemoveItem, player, removePacket);


    }
    public static void C_ChatHandler(PacketSession session, IMessage packet)
    {
		C_Chat removePacket = (C_Chat)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleChat, player, removePacket);


    }
}

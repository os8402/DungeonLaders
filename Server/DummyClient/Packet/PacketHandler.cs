using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    //step 4
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
   

    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

    }
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;

    }
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;

    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

    }

    //step 1 
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();
        ServerSession serverSession = (ServerSession)session;

        loginPacket.UniqueId = serverSession.DummyId.ToString("0000");
        serverSession.Send(loginPacket);
    }

    // step 2 로그인 Ok + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        ServerSession serverSession = (ServerSession)session;

        //TODO : 로비 UI에서 캐릭터목록 + 캐릭터 선택
        //3직업중 랜덤하게 아무나 들어가면 됨
        Random rand = new Random();
        int idx = rand.Next(0, loginPacket.Players.Count);
  
        if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"{serverSession.DummyId.ToString("000")}";
            serverSession.Send(createPacket);
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = loginPacket.Players[idx].Name;
            serverSession.Send(enterGamePacket);
        }

    }

    //step 3 
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {

        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
        ServerSession serverSession = (ServerSession)session;

        Random rand = new Random();
        int idx = rand.Next(0, createOkPacket.Players.Count);

        //3직업중 랜덤하게 아무나 들어가면 됨

        if (createOkPacket.Players == null || createOkPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"{serverSession.DummyId.ToString("000")}";
            serverSession.Send(createPacket);
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Players[idx].Name;
            serverSession.Send(enterGamePacket);
        }



    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;

    }
    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem itemList = (S_AddItem)packet;

    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItemOk = (S_EquipItem)packet;

    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat equipOk = (S_ChangeStat)packet;
        //TODO 

    }
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
    }
    public static void S_GetExpHandler(PacketSession session, IMessage packet)
    {
        S_GetExp expPacket = (S_GetExp)packet;
    }
    public static void S_LevelUpHandler(PacketSession session, IMessage packet)
    {
        S_LevelUp upPacket = (S_LevelUp)packet;
    }
    public static void S_UseItemHandler(PacketSession session, IMessage packet)
    {
        S_UseItem useItemPacket = (S_UseItem)packet;
    }
    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        S_RemoveItem removeItemPacket = (S_RemoveItem)packet;
    }
}
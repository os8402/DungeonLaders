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
        ServerSession serverSession = (ServerSession)session;

        // [2026-09 추가] C_EnterGame → S_EnterGame 왕복 시간 — 첫 번째 S_EnterGame 만 잰다.
        // 더미는 가만히 서 있어서 몬스터에게 죽고, 서버가 리스폰할 때 S_EnterGame 을 다시 보낸다.
        // 그 뒤의 S_EnterGame 은 리스폰 횟수로만 센다 (첫 실행에서 inGame 이 230 을 넘어 계속 늘어난 이유).
        if (serverSession.EnterSentTick > 0)
        {
            DummyClient.LoadStats.RecordEnter(Environment.TickCount64 - serverSession.EnterSentTick);
            serverSession.EnterSentTick = 0;
        }
        else
        {
            DummyClient.LoadStats.RecordRespawn();
        }
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
    public static void S_DamagedHandler(PacketSession session, IMessage packet)
    {
        S_Damaged damagedPacket = packet as S_Damaged;

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

        // 예전엔 UniqueId 에 DummyId 문자열만 넣었고 서버가 그걸 그대로 믿었다.
        // 이제 AccountServer 가 발급한 (AccountDbId, Token) 을 보내야 로비로 들어갈 수 있다.
        loginPacket.AccountDbId = serverSession.AccountDbId;
        loginPacket.Token = serverSession.Token;
        serverSession.LoginSentTick = Environment.TickCount64;   // [2026-09 추가] RTT 측정
        serverSession.Send(loginPacket);
}

    // step 2 로그인 Ok + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        ServerSession serverSession = (ServerSession)session;

        if (loginPacket.LoginOk == 0)
        {
            // 토큰 검증 실패 — 서버가 약 1초 뒤 연결을 끊는다 (--bogus 모드에서는 이게 정상)
            DummyClient.LoadStats.RecordLoginFail();
            Console.WriteLine($"[{serverSession.DummyId:0000}] 로그인 거부 (LoginOk=0)");
            return;
        }

        // [2026-09 추가] C_Login → S_Login 왕복 시간
        DummyClient.LoadStats.RecordLogin(Environment.TickCount64 - serverSession.LoginSentTick);

        //TODO : 로비 UI에서 캐릭터목록 + 캐릭터 선택
        //3직업중 랜덤하게 아무나 들어가면 됨
        Random rand = new Random();
        int idx = rand.Next(0, loginPacket.Players.Count);

        if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            // 계정마다 다른 이름이어야 한다 — 서버는 플레이어 이름이 겹치면 빈 S_CreatePlayer 를 돌려준다
            createPacket.Name = $"dummy{serverSession.AccountDbId}";
            serverSession.Send(createPacket);
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = loginPacket.Players[idx].Name;
            serverSession.EnterSentTick = Environment.TickCount64;
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
            // 이름 중복으로 생성이 거부된 것. 원래는 같은 이름으로 무한 재시도했다 — 한 번만 알리고 포기한다.
            Console.WriteLine($"[{serverSession.DummyId:0000}] 캐릭터 생성 실패 (이름 중복) — 이 세션은 로비에 머문다");
            return;
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Players[idx].Name;
            serverSession.EnterSentTick = Environment.TickCount64;
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
    public static void S_ChangeWeaponHandler(PacketSession session, IMessage packet)
    {
        S_ChangeWeapon changePacket = (S_ChangeWeapon)packet;
    }
    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
        S_Chat chatPacket = (S_Chat)packet;

    }
}
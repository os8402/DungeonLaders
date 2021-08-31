using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Data;

class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);

    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
        Managers.Object.Clear();

    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
        }

    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }

    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;


        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        if (bc.CL_STATE == ControllerState.Dead)
            return;

        bc.PosInfo = movePacket.PosInfo;


    }
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;

        GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();

        if (cc != null && cc.CL_STATE != ControllerState.Dead)
        {
            cc.UseSkill(skillPacket);
        }


    }

    public static void S_DamagedHandler(PacketSession session, IMessage packet)
    {
        S_Damaged damagedPacket = packet as S_Damaged;

        GameObject go = Managers.Object.FindById(damagedPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();

        if (cc != null)
        {

            UI_HitDamage hitUI = Managers.UI.MakeWorldSpaceUI<UI_HitDamage>(cc.transform);
            hitUI.Creature = cc;
            hitUI.Damage = damagedPacket.Damage;
            hitUI.RefreshUI();

            cc.TotalHp = damagedPacket.TotalHp;
            cc.Hp = damagedPacket.Hp;

            cc.OnDamaged();
        }

    }


    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;


        GameObject go = Managers.Object.FindById(changePacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();

        if (cc == null)
            return;

        cc.TotalHp = changePacket.TotalHp;
        cc.Hp = changePacket.Hp;

    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();

        if (cc != null)
        {
            cc.Hp = 0;
            cc.OnDead();
        }

    }
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_ConnectedHandler");
        C_Login loginPacket = new C_Login();
        string path = Application.dataPath;
        loginPacket.UniqueId = path.GetHashCode().ToString();
        Managers.Network.Send(loginPacket);
        Managers.Scene.LoadScene(Define.Scene.Lobby);

    }

    // 로그인 Ok + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        Debug.Log($"LoginOk_{loginPacket.LoginOk}");



        //TODO : 로비 UI에서 캐릭터목록 + 캐릭터 선택
        if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"{Random.Range(0, 1000).ToString("000")}";

            Managers.Network.Send(createPacket);
        }
        else
        {

            foreach (LobbyPlayerInfo player in loginPacket.Players)
            {
                int lastIndex = player.Name.LastIndexOf('_');
                string subStr = player.Name.Substring(0, lastIndex);
                string jobName = subStr.Replace("My", string.Empty);
                GameObject go = Managers.Resource.Instantiate($"Npc/{jobName}_Npc");
                if (go == null)
                    continue;

                SelectNpc npc = go.GetComponent<SelectNpc>();
                if (npc == null)
                    continue;

                npc.JobName = jobName;
                npc.PlayerInfo = player;

            }

        }

    }
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {

        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;

        if (createOkPacket.Players == null || createOkPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"{Random.Range(0, 1000).ToString("000")}";
            Managers.Network.Send(createPacket);

        }
        else
        {
            foreach (LobbyPlayerInfo player in createOkPacket.Players)
            {
                int lastIndex = player.Name.LastIndexOf('_');
                string subStr = player.Name.Substring(0, lastIndex);
                string jobName = subStr.Replace("My", string.Empty);
                GameObject go = Managers.Resource.Instantiate($"Npc/{jobName}_Npc");
                if (go == null)
                    continue;

                SelectNpc npc = go.GetComponent<SelectNpc>();
                if (npc == null)
                    continue;

                npc.JobName = jobName;
                npc.PlayerInfo = player;

            }
        }

    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;

        Managers.Inven.Clear();

        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);

        }

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshCalcStat();
    }
    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem itemList = (S_AddItem)packet;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);
            gameSceneUI.NewsUI.RefreshUI($"아이템을 얻었습니다.(+{item.ItemName})");

        }

        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshCalcStat();

    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItemOk = (S_EquipItem)packet;

        //메모리에 저장
        Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
        if (item == null)
            return;

        item.Equipped = equipItemOk.Equipped;

        if (Managers.Object.MyPlayer == null)
            return;

        Managers.Object.MyPlayer.RefreshCalcStat();

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();



    }
    public static void S_ChangeWeaponHandler(PacketSession session, IMessage packet)
    {
        S_ChangeWeapon changePacket = (S_ChangeWeapon)packet;

        GameObject go = Managers.Object.FindById(changePacket.ObjectId);

        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();

        cc.DestroyWeapon();

        if (changePacket.Equipped)
        {
            cc.CreateWeapon(changePacket.TemplateId);
        }



        Managers.Object.MyPlayer.RefreshCalcStat();

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat equipOk = (S_ChangeStat)packet;
        //TODO 

    }
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
        Debug.Log("[Server] Ping Check");
        Managers.Network.Send(pongPacket);
    }
    public static void S_GetExpHandler(PacketSession session, IMessage packet)
    {
        S_GetExp expPacket = (S_GetExp)packet;

        GameObject go = Managers.Object.FindById(expPacket.ObjectId);
        if (go == null)
            return;

        MyPlayerController player = go.GetComponent<MyPlayerController>();

        if (player != null)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

            gameSceneUI.NewsUI.RefreshUI($"경험치를 획득했습니다.(+{ expPacket.Exp - player.Exp})");
            player.Exp = expPacket.Exp;


            if (player.Stat.CurExp >= player.Stat.TotalExp)
            {
                gameSceneUI.NewsUI.RefreshUI($"레벨업 했습니다! (+{player.Stat.Level} -> {player.Stat.Level + 1})");
                C_LevelUp levelUpPacket = new C_LevelUp();
                Managers.Network.Send(levelUpPacket);

            }

        }

    }
    public static void S_LevelUpHandler(PacketSession session, IMessage packet)
    {
        S_LevelUp upPacket = (S_LevelUp)packet;

        GameObject go = Managers.Object.FindById(upPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
            return;

        cc.Stat.MergeFrom(upPacket.StatInfo);

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        cc.LevelUp();

        cc.UpdateHpBar();
        cc.UpdateCharInfoUI();

        gameSceneUI.StatUI.RefreshUI();
        gameSceneUI.StatusUI.RefreshUI();


    }

    public static void S_UseItemHandler(PacketSession session, IMessage packet)
    {
        S_UseItem useItemOk = (S_UseItem)packet;

        //메모리에 저장
        Item useItem = Managers.Inven.Get(useItemOk.Item.ItemDbId);
        if (useItem == null)
            return;

        int count = useItemOk.Item.Count;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        //다썻다면..
        if (count <= 0)
        {
            Managers.Inven.Remove(useItem);

            var sortingItems = Managers.Inven.Items.Values
                      .Where(i => i.Slot > useItem.Slot)
                      .ToList();

            foreach (Item i in sortingItems)
                Managers.Inven.RefreshSlot(i, i.Slot - 1);

            //싹 다 날리고 초기화
            gameSceneUI.InvenUI.MakeItem();

        }
        //아직 남아있다면..
        else
        {
            useItem.Count = useItemOk.Item.Count;
            gameSceneUI.InvenUI.RefreshUI();

        }


        gameSceneUI.StatUI.RefreshUI();

    }

    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        S_RemoveItem removeOk = (S_RemoveItem)packet;



        //메모리에 저장
        Item removeItem = Managers.Inven.Get(removeOk.ItemDbId);
        if (removeItem == null)
            return;

        Managers.Inven.Remove(removeItem);

        var sortingItems = Managers.Inven.Items.Values
                  .Where(i => i.Slot > removeItem.Slot)
                  .ToList();

        foreach (Item i in sortingItems)
            Managers.Inven.RefreshSlot(i, i.Slot - 1);

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        //싹 다 날리고 초기화
        gameSceneUI.InvenUI.MakeItem();
        gameSceneUI.StatUI.RefreshUI();

    }
}

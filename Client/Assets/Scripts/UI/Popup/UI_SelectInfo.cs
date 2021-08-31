using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectInfo : UI_Popup
{
    
    enum Images
    {
        Slot_Job
    }

    enum Texts
    {
        NameText,
        LevelText,
        Hp_ValueText,
        Attack_ValueText,
        Defence_ValueText,
        Speed_ValueText,
    }
    enum Buttons
    {
        Exit_Button_Bg,
        Exit_Button,
        Select_Button
    }

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

    }

    public void RefreshUI(SelectNpc npc)
    {
        LobbyPlayerInfo player = npc.PlayerInfo;

        BindEvent(GetButton((int)Buttons.Exit_Button_Bg).gameObject, (e) => { ClosePopupUI(); });
        BindEvent(GetButton((int)Buttons.Exit_Button).gameObject, (e) => { ClosePopupUI(); });

        GetImage((int)Images.Slot_Job).sprite = npc.GetComponent<SpriteRenderer>().sprite;
        GetText((int)Texts.NameText).text = $"{npc.JobName}";

        float expRatio = 0.0f;
        expRatio = ((float)player.StatInfo.CurExp / player.StatInfo.TotalExp);

        GetText((int)Texts.LevelText).text = $"LV{player.StatInfo.Level}({expRatio.ToString("F2")})";


        WeaponData weapon = null;
        ArmorData helmet = null;
        ArmorData armor = null;
        ArmorData boots = null; 
    
        foreach (int templateId in player.EquippedItemList)
        {
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData == null)
                continue;


            switch(itemData.itemType)
            {
                case ItemType.Weapon:
                    weapon = itemData as WeaponData; 
                    break;
                case ItemType.Armor:

                    ArmorData armorData = itemData as ArmorData;

                    if (armorData.armorType == ArmorType.Helmet)
                        helmet = armorData;
                    else if (armorData.armorType == ArmorType.Armor)
                        armor = armorData;
                    else if (armorData.armorType == ArmorType.Boots)
                        boots = armorData;

                    break;
        
            }
            


        }


        //공격력 
        int totalDmg = (weapon == null) ?  player.StatInfo.Attack  : player.StatInfo.Attack + weapon.damage;
        GetText((int)Texts.Attack_ValueText).text = $"{totalDmg}+({ totalDmg -  player.StatInfo.Attack})";


        //방어력
        int totalDef = 0;
        if (helmet != null) totalDef += helmet.defence;
        if (armor != null) totalDef += armor.defence;
        if (boots != null) totalDef += boots.defence;
        GetText((int)Texts.Defence_ValueText).text = $"{totalDef}+({totalDef})";


        //체력 
        int totalHp = player.StatInfo.MaxHp;
        if (helmet != null) totalHp += helmet.hp;
        if (armor != null) totalHp += armor.hp;
        if (boots != null) totalHp += boots.hp;
        GetText((int)Texts.Hp_ValueText).text = $"{player.StatInfo.Hp}/{totalHp}";

        //이동 속도
        float totalSpeed = player.StatInfo.Speed;
        if (helmet != null) totalSpeed += helmet.speed;
        if (armor != null) totalSpeed += armor.speed;
        if (boots != null) totalSpeed += boots.speed;
        GetText((int)Texts.Speed_ValueText).text = $"{totalSpeed}+({totalSpeed -  player.StatInfo.Speed})";

        BindEvent(GetButton((int)Buttons.Select_Button).gameObject, (e) =>
        {
            Managers.Scene.LoadScene(Define.Scene.Game);
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = player.Name;
            Managers.Network.Send(enterGamePacket);

        });

    }

}

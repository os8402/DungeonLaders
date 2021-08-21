using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Base
{
    enum Images
    {
        Slot_Helmet,
        Slot_Weapon,
        Slot_Armor,
        Slot_Shield,
        Slot_Boots
    }
    enum Texts
    {
        Level_ValueText,
        Job_ValueText,
        Hp_ValueText,
        Attack_ValueText,
        Defence_ValueText,
        Speed_ValueText,
        Exp_ValueText,
    }

    bool _init = false; 
    public override void Init()
    {

        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        _init = true; 

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_init == false)
            return;

        Get<Image>((int)Images.Slot_Helmet).enabled = false;   
        Get<Image>((int)Images.Slot_Weapon).enabled = false;   
        Get<Image>((int)Images.Slot_Armor).enabled = false;   
        Get<Image>((int)Images.Slot_Shield).enabled = false;   
        Get<Image>((int)Images.Slot_Boots).enabled = false;   

        foreach(Item item in Managers.Inven.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

            if(item.ItemType == ItemType.Weapon)
            {
                Get<Image>((int)Images.Slot_Weapon).enabled = true;
                Get<Image>((int)Images.Slot_Weapon).sprite = icon;
            }
            else if(item.ItemType == ItemType.Armor)
            {
                Armor armor = (Armor)item;
                switch (armor.ArmorType)
                {
                    case ArmorType.Helmet:
                        Get<Image>((int)Images.Slot_Helmet).enabled = true;
                        Get<Image>((int)Images.Slot_Helmet).sprite = icon;
                        break;
                    case ArmorType.Armor:
                        Get<Image>((int)Images.Slot_Armor).enabled = true;
                        Get<Image>((int)Images.Slot_Armor).sprite = icon;
                        break;
                    case ArmorType.Boots:
                        Get<Image>((int)Images.Slot_Boots).enabled = true;
                        Get<Image>((int)Images.Slot_Boots).sprite = icon;
                        break;
                }

            }

        }

        //Text
        MyPlayerController player = Managers.Object.MyPlayer;
        if (player == null)
            return;

        player.RefreshCalcStat();

        GetText((int)Texts.Level_ValueText).text = $"{player.Stat.Level}";
        GetText((int)Texts.Job_ValueText).text = $"전사";
        int totalDmg = player.Stat.Attack + player.MyWeapon.WeaponDamage;
        GetText((int)Texts.Attack_ValueText).text = $"{totalDmg} + ({player.MyWeapon.WeaponDamage})";
        GetText((int)Texts.Defence_ValueText).text = $"{player.ArmorDefence}";
        GetText((int)Texts.Hp_ValueText).text = $"{player.Hp}/{player.Stat.MaxHp}";
        GetText((int)Texts.Speed_ValueText).text = $"{player.Speed}";
        GetText((int)Texts.Exp_ValueText).text = $"{player.Exp}/{player.Stat.TotalExp}";


    }
}

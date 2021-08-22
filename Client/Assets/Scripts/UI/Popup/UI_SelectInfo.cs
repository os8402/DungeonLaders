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

        //int totalDmg = player.Stat.Attack + player.MyWeapon.WeaponDamage;
        //GetText((int)Texts.Attack_ValueText).text = $"{totalDmg} + ({player.MyWeapon.WeaponDamage})";
      //  GetText((int)Texts.Defence_ValueText).text = $"{player.StatInfo.Attack}";

        GetText((int)Texts.Attack_ValueText).text = $"{player.StatInfo.Attack}";
        GetText((int)Texts.Defence_ValueText).text = $"{player.StatInfo.Attack}";

        int hp = (player.StatInfo.Hp == 0) ? player.StatInfo.MaxHp : player.StatInfo.Hp;
        GetText((int)Texts.Hp_ValueText).text = $"{hp}/{player.StatInfo.MaxHp}";
        GetText((int)Texts.Speed_ValueText).text = $"{player.StatInfo.Speed}";

        BindEvent(GetButton((int)Buttons.Select_Button).gameObject, (e) =>
        {
            Managers.Scene.LoadScene(Define.Scene.Game);
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = player.Name;
            Managers.Network.Send(enterGamePacket);

        });

    }

}

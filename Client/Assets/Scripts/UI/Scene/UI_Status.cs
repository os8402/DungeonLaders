using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UI_Status : UI_Base
{
    enum Images
    {
        Job_Icon,
        Hp_Bar,
        Mp_Bar,
        Exp_Bar
    }
    enum Texts
    {
        Hp_ValueText,
        Mp_ValueText,
        Exp_ValueText,
        Lv_ValueText,
   
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
    }

   
    

    public void RefreshUI()
    {
        MyPlayerController player = Managers.Object.MyPlayer;
        if (player == null)
            return;

        int lastIndex = player.name.LastIndexOf('_');
        string subName = player.name.Substring(0, lastIndex);
        subName = subName.Replace("My", string.Empty);

        GetImage((int)Images.Job_Icon).GetComponent<Animator>().Play($"{subName}_Icon");

        float hpRatio = 0.0f;
        if (player.Stat.MaxHp > 0)
            hpRatio = ((float)player.Hp / player.Stat.MaxHp);

        float mpRatio = 0.0f;
        if (player.Stat.MaxMp > 0)
            mpRatio = ((float)player.Mp / player.Stat.MaxMp);

        float expRatio = 0.0f;
        expRatio = ((float)player.Exp / player.Stat.TotalExp);


        GetImage((int)Images.Hp_Bar).fillAmount = hpRatio;
        GetImage((int)Images.Mp_Bar).fillAmount = mpRatio;
        GetImage((int)Images.Exp_Bar).fillAmount = expRatio;

        GetText((int)Texts.Hp_ValueText).text = $"{player.Hp}/{player.Stat.MaxHp}";
        GetText((int)Texts.Mp_ValueText).text = $"{player.Mp}/{player.Stat.MaxMp}";
        GetText((int)Texts.Exp_ValueText).text = $"{player.Exp}/{player.Stat.TotalExp} ({(expRatio * 100).ToString("F2")}%)";

        GetText((int)Texts.Lv_ValueText).text = $"LV {player.Stat.Level}";
    }
}

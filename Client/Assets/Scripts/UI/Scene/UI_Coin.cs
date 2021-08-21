using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Coin : UI_Base
{

    enum Texts
    {
        Coin_Text

    }
    public override void Init()
    {
        Bind<Text>(typeof(Texts));
    }

    public void RefreshUI()
    {
        GetText((int)Texts.Coin_Text).text = "0g";
    }

}

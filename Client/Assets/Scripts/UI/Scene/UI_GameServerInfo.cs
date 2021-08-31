using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameServerInfo : UI_Base
{
    enum Texts
    {
        ServerInfoText
    }

    public override void Init()
    {
        Bind<Text>(typeof(Texts));

        GetText((int)Texts.ServerInfoText).text = $"현재 서버 : {Managers.Network.ServerName}"; 
    }
}

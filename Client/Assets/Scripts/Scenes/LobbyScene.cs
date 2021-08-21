using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        Screen.SetResolution(640, 480, false);

        SceneType = Scene.Lobby;

        //Json 로딩
        Managers.Data.Init();


    }
    public override void Clear()
    {
   
    }

}

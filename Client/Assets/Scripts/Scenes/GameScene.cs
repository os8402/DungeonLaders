using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    UI_GameScene _sceneUI; 

    protected override void Init()
    {
        base.Init();

        Screen.SetResolution(640, 480, false); 


        SceneType = Scene.Game;

        //Json 로딩
        Managers.Data.Init();
        //맵 로딩
        Managers.Map.LoadMap(1);

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();

    }

 

    public override void Clear()
    {
        
    }
}

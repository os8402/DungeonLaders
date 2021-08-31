using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class LoginScene : BaseScene
{
    UI_LoginScene _sceneUI; 

    protected override void Init()
    {
        base.Init();
        Screen.SetResolution(1280, 720, false);

        SceneType = Scene.Login;



        _sceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>(); 

    }

    public override void Clear()
    {
      
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        Screen.SetResolution(1280, 720, false);

        SceneType = Scene.Login;

        Managers.UI.ShowPopupUI<UI_Loading>();
    }

    public override void Clear()
    {
      
    }
}

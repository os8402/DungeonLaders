using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Managers.Map.LoadMap(1);

        int idx = 1; 
        Managers.Resource.Instantiate("Character/Warrior" , name : $"Warrior_{idx.ToString("000")}");
        Managers.Resource.Instantiate("Character/Skeleton" , name : $"Skeleton_{idx.ToString("000")}");



    }

    public override void Clear()
    {
        
    }
}

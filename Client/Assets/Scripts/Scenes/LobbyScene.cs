using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class LobbyScene : BaseScene
{
   
    protected override void Init()
    {
        base.Init();
        SceneType = Scene.Lobby;

        // Db에서 플레이어 데이터 로딩 대기
        //  받았으면 그 후에 처리.. 

        Managers.Map.LoadMap(2);

        Managers.Resource.Instantiate("Common/Camp");


    }
    public override void Clear()
    {
   
    }

}

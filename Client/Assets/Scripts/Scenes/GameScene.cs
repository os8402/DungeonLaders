using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        Screen.SetResolution(640, 480, false); 


        SceneType = Scene.Game;

        Managers.Map.LoadMap(1);

        //PlayerInfo info = new PlayerInfo()
        //{
           
        //    PlayerId = 1,
        //    PosInfo = new PositionInfo() { PosX = 0 , PosY = 0 , Dir = 0 , State = ControllerState.Idle }
            
        //};
      
        //Managers.Object.CreateCreature("MyWarrior", info, Weapons.Spear);

        //Managers.Object.CreateCreature("MyWarrior", idx ,  teamId , Weapons.Spear);

        //teamId = 2 << 24;

        //for (; idx < 15; idx++)
        //{
        //    if(idx % 2 == 0)
        //        Managers.Object.CreateCreature("Skeleton", idx, teamId , Weapons.Spear); 
        //    else
        //        Managers.Object.CreateCreature("Skeleton", idx , teamId , Weapons.Sword);
        //}

    }

 

    public override void Clear()
    {
        
    }
}

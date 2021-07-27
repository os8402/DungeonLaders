using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Scene.Game;

        Managers.Map.LoadMap(1);

        int idx = 1;
        int teamId = 1 << 24;

        Managers.Object.CreateCreature("Warrior", idx ,  teamId , Weapons.Spear);

        teamId = 2 << 24;

        for (; idx < 15; idx++)
        {
            if(idx % 2 == 0)
                Managers.Object.CreateCreature("Skeleton", idx, teamId , Weapons.Spear); 
            else
                Managers.Object.CreateCreature("Skeleton", idx , teamId , Weapons.Sword);
        }

    }

 

    public override void Clear()
    {
        
    }
}

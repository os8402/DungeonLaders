using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Managers.Map.LoadMap(1);

        int idx = 1; 
        GameObject player = Managers.Resource.Instantiate("Character/Warrior" , name : $"Warrior_{idx.ToString("000")}");
        Managers.Object.Add(player);
        PlayerController pc = player.GetComponent<PlayerController>();
        pc.CreateWeapon(Weapons.Sword , 1);

        for (; idx < 6; idx++)
        {
            Vector3Int initPos = new Vector3Int
            {
                x = Random.Range(Managers.Map.MinX + 1, Managers.Map.MaxX - 1),
                y = Random.Range(Managers.Map.MinY + 1, Managers.Map.MaxY - 1),
            };

            GameObject monster = Managers.Resource.Instantiate("Character/Skeleton", name: $"Skeleton_{idx.ToString("000")}");

            MonsterController mc = monster.GetComponent<MonsterController>();
            mc.Pos = initPos;
            mc.CreateWeapon(Weapons.Spear, 1);
            mc.Id = idx; 

            Managers.Object.Add(monster);
        }

    }

    public override void Clear()
    {
        
    }
}

using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    void GetDirRot()
    {
       
        switch (Dir)
        {
            case DirState.Up:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                break;
            case DirState.Down:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                break;
            case DirState.Left:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case DirState.Right:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                break;
            case DirState.UpLeft:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, -45));
                break;
            case DirState.DownLeft:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 45));
                break;
            case DirState.DownRight:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 135));
                break;
            case DirState.UpRight:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, -135));
                break;
        }
    }

    protected override void Init()
    {
        GetDirRot();
        Speed = 15f;
    }

    void Start()
    {
        Init();
    }


    //혹시 몰라서 다 막음
    protected override void UpdateAnimation() { }
    public override void OnDamaged(GameObject attacker, int damage) { }
    public override void OnDead(GameObject attacker) { }

}

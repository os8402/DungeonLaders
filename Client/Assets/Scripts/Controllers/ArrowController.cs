using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : BaseController
{

    public override PositionInfo PosInfo
    {
        get { return base.PosInfo; }
        set
        {
            base.PosInfo = value;

            Managers.Map.VisibleCellEffect(CellPos , this);
        }
    }

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
    }

    void Start()
    {
        Init();
    }

    protected override void UpdateAnimation() { }

    protected override void UpdateRotation() { }

    protected override void UpdateIdle() { }
 
    protected override void MoveToNextPos() { }
  
}

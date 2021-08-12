using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Sword : BaseWeapon
{
    //TODO : 검은 공격할 때 방향에 따라 Sorting Order도 적용
    private int _swordDir = 0;

    public int SwordDir
    {
        get { return _swordDir; }
        set
        {
            if (_owner == null)
                return;

            _swordDir = value;
            int ownerOrder = _owner.SpriteRenderer.sortingOrder;
            int myOrder = (_swordDir == 1 ? ownerOrder + 5 : ownerOrder - 5);
            _spriteRenderer.sortingOrder = myOrder;
        }
    }
    void Awake()
    {
        Init();

    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 1;
    }

    protected override void UpdateRotation()
    {

        if (_owner.Dir == DirState.Left)
        {
            //무기의 위치 , 마우스 위치
            switch (_swordDir)
            {
                //Up
                case 0:
                    _q = Util.LookAt2D(transform.position, _targetPos);
                    break;
                case 1:
                    _q = Util.LookAt2D(_targetPos, transform.position);
                    break;
            }
        }
        else
        {
            switch (_swordDir)
            {
                case 0:

                    _q = Util.LookAt2D(_targetPos, transform.position);
                    break;
                case 1:
                    _q = Util.LookAt2D(transform.position, _targetPos);
                    break;
            }
        }

        transform.rotation = _q;
    }

    public override void SkillEvent(S_Skill skillPacket)
    {

        base.SkillEvent(skillPacket);

        SwordDir = (1 - SwordDir);  // -1 연산 

        //보여주기용 좌표
        _ec.transform.localPosition = new Vector2(_attackDir.AttkPosX * 0.5f, _attackDir.AttkPosY * 0.5f);
        _ec.transform.localRotation = _rot;
        _ec = null;

    }


}

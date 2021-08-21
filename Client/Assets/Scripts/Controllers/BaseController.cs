using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public abstract  class BaseController : MonoBehaviour
{
    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    protected Animator _animator;

    protected int _id;
    public int Id { get { return _id; } set { _id = value; } }
    public int TeamId { get; set; }

    //  CellPos , CL_STATE , Dir이 갱신되면 True로 바꾸고 패킷전송
    protected bool _updated = false; 

    protected bool _ignoreCollision = false;

    protected StatInfo _stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.Level = value.Level;
            _stat.Hp = value.Hp;
            _stat.MaxHp = value.MaxHp;
            _stat.Mp = value.Mp;
            _stat.MaxMp = value.MaxMp;
            _stat.MaxMp = value.MaxMp;
            _stat.Attack = value.Attack;
            _stat.Speed = value.Speed;
            _stat.CurExp = value.CurExp;
            _stat.TotalExp = value.TotalExp;
        }
    }
    public  float Speed
    {
        get { return Stat.Speed; }
        set { Stat.Speed = value;  }
    }

    protected PositionInfo _positionInfo = new PositionInfo { Target = new TargetInfo() };
    public virtual PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            CellPos = new Vector3Int(value.PosX, value.PosY,0);
            CL_STATE = value.State;
            Target = value.Target;
      

        }
    }



    public TargetInfo Target
    {
        get { return PosInfo.Target; }
        set
        {
            if (PosInfo.Target.Equals(value))
                return;


            TargetPos = new Vector3(value.TargetX, value.TargetY);
            Dir = value.Dir;
        }
    } 
    public Vector3 TargetPos
    {
        get { return new Vector3(PosInfo.Target.TargetX, PosInfo.Target.TargetY); }
        set
        {
            if (PosInfo.Target.TargetX.Equals(value.x) && PosInfo.Target.TargetY.Equals(value.y))
                return;

            PosInfo.Target.TargetX = value.x;
            PosInfo.Target.TargetY = value.y;

        }
    }

    public virtual DirState Dir
    {

        get { return PosInfo.Target.Dir; }
        set
        {
            if (PosInfo.Target.Dir.Equals(value))
                return;

            PosInfo.Target.Dir = value;

            //초기화가 안된 것
            if (_spriteRenderer == null)
                return;

            string name = Enum.GetName(typeof(DirState) , Dir);
          
           _spriteRenderer.flipX = (name.Contains("Right") ? true : false);
   
         
            UpdateAnimation();
            _updated = true;
            CheckUpdatedFlag();

        }
    }

    public virtual ControllerState CL_STATE
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State.Equals(value))
                return;

            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }

    //1. State 체크
    //2. 애니메이션 변경 
    //3.관련 기능 매핑해둘 것

    public Vector3Int CellPos
    {
        get { return new Vector3Int(PosInfo.PosX, PosInfo.PosY, 0); }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
            
            UpdateAnimation();
        
        }
    }


    protected virtual void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        

        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }
    public void SyncPos()
    {
        
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = destPos;
    }

    void Update()
    {
   
        UpdateController();      
    }

    protected Quaternion _q;
    protected abstract void UpdateRotation();
    protected virtual void UpdateController()
    {
        switch(CL_STATE)
        {
            case ControllerState.Idle:
                UpdateIdle();
                UpdateRotation();
                break;
            case ControllerState.Moving:
                UpdateMoving();
                UpdateRotation();
                break;
            case ControllerState.Skill:
                UpdateSkill();
                UpdateRotation();
                break;
            case ControllerState.Dead:
                
                break;

        }
    }

    protected abstract void UpdateIdle();
    //다음목적지까지 어떻게 갈건지는 컨트롤러마다 달라서 상속받음
    protected abstract void MoveToNextPos();

   
    protected virtual void UpdateMoving()
    {
        
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;


        float dist = moveDir.magnitude;

        if (dist < Speed * Time.deltaTime)
        {
            transform.position = destPos;
            MoveToNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
            CL_STATE = ControllerState.Moving;
        }
    }


    protected virtual void UpdateSkill()
    { 

    }

    protected virtual void UpdateAnimation()
    {
        if (_animator == null)
            return;

        //오브젝트의 이름을 알기위해
        //나중에 클래스가 추가되면 수정
        int lastIndex = gameObject.name.LastIndexOf('_');
        string subName = gameObject.name.Substring(0, lastIndex);
        subName = subName.Replace("My", string.Empty);


        switch (CL_STATE)
        {
            case ControllerState.Idle:
                _animator.Play($"{subName}_Idle");
                break;
            case ControllerState.Moving:
                _animator.Play($"{subName}_Moving");
                break;
            case ControllerState.Skill:
                _animator.Play($"{subName}_Skill");
                break;
            case ControllerState.Dead:
                _animator.Play($"{subName}_Dead");
                break;
        }
    }

    protected virtual void CheckUpdatedFlag() { }

}

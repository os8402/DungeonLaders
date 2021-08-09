using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public abstract  class BaseController : MonoBehaviour
{
    protected int id;
    public int Id { get { return id; } set { id = value; } }

    [SerializeField]
    //  CellPos , CL_STATE , Dir이 갱신되면 True로 바꾸고 패킷전송
    protected bool _updated = false; 
    protected float Speed { get;  set; } = 10.0f;
    protected bool _ignoreCollision = false; 

    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    protected Animator _animator;

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            CellPos = new Vector3Int(value.PosX, value.PosY,0);
            CL_STATE = value.State;
            Dir = value.Dir;

        }
    }

    //1.여기서 방향을 받는 김에 회전도 같이 설정
    //2.마우스 위치를 기준으로 합니다. 
    //3.관련 기능 매핑해둘 것
  
    public virtual int Dir
    {

        get { return PosInfo.Dir; }
        set
        {
            if (PosInfo.Dir.Equals(value))
                return;

            PosInfo.Dir = value;

            //초기화가 안된 것
            if (_spriteRenderer == null)
                return;

           _spriteRenderer.flipX = (Dir == 1 ? true : false);
            float flipY = (Dir == 1 ? 180 : 0);
         
            UpdateAnimation();
            _updated = true;

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

       //     if (_ignoreCollision == false)
          //   Managers.Map.SetPosObject(CellPos, null);

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;

        //    if (_ignoreCollision == false)
            // Managers.Map.SetPosObject(CellPos, gameObject);
            
            UpdateAnimation();
        
        }
    }
    public Vector3 TargetPos { get; set; }

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

}

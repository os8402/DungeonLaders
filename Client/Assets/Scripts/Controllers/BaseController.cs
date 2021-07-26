using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public abstract  class BaseController : MonoBehaviour
{
    protected int id;
    public int Id { get { return id; } set { id = value; } }

    protected float Speed { get;  set; } = 10.0f;
    protected bool _ignoreCollision = false; 

    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    protected Animator _animator;

    //1.여기서 방향을 받는 김에 회전도 같이 설정
    //2.마우스 위치를 기준으로 합니다. 
    //3.관련 기능 매핑해둘 것
    protected int _dir = 0;
    public virtual int Dir
    {

        get { return _dir; }
        set
        {
            _dir = value;

            //초기화가 안된 것
            if (_spriteRenderer == null)
                return;

           _spriteRenderer.flipX = (_dir == 1 ? true : false);
            float flipY = (_dir == 1 ? 180 : 0);
         
            UpdateAnimation();

        }
    }

    [SerializeField]
    protected ControllerState _cl_state = ControllerState.Idle;
    public virtual ControllerState CL_STATE
    {
        get { return _cl_state; }
        set
        {
            _cl_state = value;
            UpdateAnimation();
        }
    }

    //1. State 체크
    //2. 애니메이션 변경 
    //3.관련 기능 매핑해둘 것
    private Vector3Int _pos;
    public Vector3Int Pos
    {
        get { return _pos; }
        set
        {
            if(_ignoreCollision == false)
             Managers.Map.SetPosObject(_pos, null);
            
            _pos = value;
            
            if(_ignoreCollision == false)
             Managers.Map.SetPosObject(_pos, gameObject);
            
            UpdateAnimation();
        }
    }


    protected virtual void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(Pos) + new Vector3(.5f, .5f);
        transform.position = pos;


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
            case ControllerState.Move:
                UpdateMoving();
                UpdateRotation();
                break;
            case ControllerState.Skill:
                UpdateSkill();
                UpdateRotation();
                break;
            case ControllerState.Death:
            
                break;

        }
    }

    protected abstract void UpdateIdle();
    //다음목적지까지 어떻게 갈건지는 컨트롤러마다 달라서 상속받음
    protected abstract void MoveToNextPos();

   
    protected virtual void UpdateMoving()
    {
        
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(Pos) + new Vector3(0.5f, 0.5f);
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
            CL_STATE = ControllerState.Move;
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

        switch (CL_STATE)
        {
            case ControllerState.Idle:
                _animator.Play($"{subName}_Idle");
                break;
            case ControllerState.Move:
                _animator.Play($"{subName}_Move");
                break;
            case ControllerState.Skill:
                _animator.Play($"{subName}_Skill");
                break;
            case ControllerState.Death:
                _animator.Play($"{subName}_Death");
                break;
        }
    }

}

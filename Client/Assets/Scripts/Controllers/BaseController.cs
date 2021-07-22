using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public abstract  class BaseController : MonoBehaviour
{
    protected int id;

    protected Grid _grid; 
    protected float Speed { get; private set; } = 10.0f;
    protected int Hp { get; private set; } = 100;

    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    protected Animator _animator;
    protected Transform _hand;
    protected BaseWeapon weapon;
    public Weapons WEAPONS { get; protected set; } = Weapons.Empty;
 

    public CreatureState CREATURE_STATE { get; protected set; } = CreatureState.Idle;
    protected Coroutine _coSkill = null;
    protected Action _skillEvent = null;

    //1. State 체크
    //2. 애니메이션 변경 
    //3.관련 기능 매핑해둘 것
    private Vector3Int _pos;
    public Vector3Int Pos
    {
        get { return _pos; }
        set
        {
            _pos = value;
            CREATURE_STATE = (value == Vector3.zero ? CREATURE_STATE = CreatureState.Idle : CreatureState.Move);
            UpdateAnimation();
        }
    }

    //1.여기서 방향을 받는 김에 회전도 같이 설정
    //2.마우스 위치를 기준으로 합니다. 
    //3.관련 기능 매핑해둘 것
    private int _dir = 0;
    public int Dir
    {

        get { return _dir; }
        set
        {
            _dir = value;

            //초기화가 안된 것
            if (_spriteRenderer == null)
                return;


            // TODO : 나중에 json에서 파싱된 값을 가져와야 함
            Vector2 hand = new Vector2(-0.2f, -0.3f);
            hand.x = (_dir == 1 ? hand.x * -1 : hand.x);
            _hand.localPosition = hand;
            _spriteRenderer.flipX = (_dir == 1 ? true : false);

        }
    }

    protected virtual void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _hand = Util.FindChild<Transform>(gameObject, "Weapon_Hand", true);
        _grid = FindObjectOfType<Grid>();

        Vector3 pos = _grid.CellToWorld(Pos) + new Vector3(.5f, .5f);
        transform.position = pos;

    }

    // 플레이어일 경우 [자신의 벡터 + 마우스 벡터] 
    // 몬스터일 경우 [자신의 벡터 + 상대 타켓의 벡터]
    // 각각 요구사항이 다르기 때문에 
    // 추상 클래스로 남겨둠
    protected abstract void UpdateRotation();
    //이동도 그러함
    protected abstract void UpdateMoving();

    protected virtual void UpdateIdle()
    {
        if (_coSkill != null)
            return;
        Pos = Vector3Int.zero;
    }


 
    void UpdateAnimation()
    {
        if (_animator == null)
            return;

        //오브젝트의 이름을 알기위해
        //나중에 클래스가 추가되면 수정
        int lastIndex = gameObject.name.LastIndexOf('_');
        string subName = gameObject.name.Substring(0, lastIndex);

        switch (CREATURE_STATE)
        {
            case CreatureState.Idle:
                _animator.Play($"{subName}_Idle");
                break;
            case CreatureState.Move:
                _animator.Play($"{subName}_Move");
                break;
            case CreatureState.Death:
                _animator.Play($"{subName}_Death");
                break;
        }
    }


}

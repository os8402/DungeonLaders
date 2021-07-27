using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    [SerializeField]
    private PlayerController _target; 
    public PlayerController Target { get { return _target; } }

    Coroutine _coPatrol;
    Coroutine _coSearch;
    [SerializeField]
    float _searchRange = 10.0f;


    public override ControllerState CL_STATE
    {
        get { return _cl_state; }
        set
        {
            base.CL_STATE = value;

            //몬스터는 무기 위치 초기화 필요
            _myWeapon.transform.localPosition = Vector2.zero;


            if(_cl_state == ControllerState.Move)
            {
                if (_target == null)
                    return;
                _target.deadTargetEvent -= CheckDeadTarget;
                _target.deadTargetEvent += CheckDeadTarget;
            }


            //스테이트 변경 시에 자동으로 취소
            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    void Start()
    {
        Init(); 
    }

    protected override void Init()
    {
        base.Init();
        Speed = 5f;
    }


    protected override void UpdateRotation()
    {

        if (_target == null)
            return;

        Quaternion q = Util.RotateDir2D(_target.transform.position, transform.position , true);

        if (q.z > Quaternion.identity.z) // 오른쪽
        {
            Dir = -1;
        }
        else if (q.z < Quaternion.identity.z)// 왼쪽
        {
            Dir = 1;
        }

        else return;
    }


    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_coSearch == null)
        {
             _coSearch = StartCoroutine("CoSearch");
        }
    }


    protected override void MoveToNextPos()
    {

        if (_target == null)
        {
            CL_STATE = ControllerState.Idle;
            return;
        }

        //스킬사용여부
        Vector3Int dir = (_target.Pos - Pos);
        int dist = Managers.Map.CellDistFromZero(dir.x, dir.y);
        int skillRange = _myWeapon.AttackRange;

        //8방향검사
        if (dist <= skillRange + 1 && Mathf.Abs(dir.x) <= skillRange && Mathf.Abs(dir.y) <= skillRange)
        {
            CL_STATE = ControllerState.Skill;
            return;
        }

        List<Vector3Int> path = Managers.Map.FindPath(Pos, _target.Pos, ignoreDestCollision : true);
        if (path.Count < 2 || (_target != null && path.Count > 20))
        {
            _target = null;
            CL_STATE = ControllerState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];
  
        if (Managers.Map.CanGo(nextPos) && Managers.Object.Find(nextPos) == null)
        {
            Pos = nextPos;
        }
        else
        {
            _target = null;
            CL_STATE = ControllerState.Idle;
        }
    }

    protected override void UpdateSkill()
    {

        
        // 유효한 타겟인지
        if (_target == null)
        { 
            CL_STATE = ControllerState.Idle;
            return;
        }

        if (_coSkill != null)
            return;

        // 스킬이 아직 사용 가능한지
        Vector3Int dir = (_target.Pos - Pos);
        int dist = Managers.Map.CellDistFromZero(dir.x , dir.y);
        int skillRange = _myWeapon.AttackRange;

        bool canUseSkill = (dist <= skillRange + 1 && Mathf.Abs(dir.x) <= skillRange && Mathf.Abs(dir.y) <= skillRange);
         

        if (canUseSkill == false)
        {
            CL_STATE = ControllerState.Move;
            _coSkill = null;
            return;
        }

        
   
         _coSkill = StartCoroutine("CoSkillAttack", 0.5f);


    }

    IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (_target != null)
                continue;

            _target = Managers.Object.Find((go) =>
            {
                if (go == null)
                    return false; 

                PlayerController pc = go.GetComponent<PlayerController>();
                if (pc == null)
                    return false;

                if (pc.Hp <= 0)
                    return false;

                Vector3Int dir = (pc.Pos - Pos);
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });

            if (_target != null)
                CL_STATE = ControllerState.Move;

        }
    }

    IEnumerator CoSkillAttack(float time)
    {
        _skillEvent?.Invoke();
        yield return new WaitForSeconds(time);
        _coSkill = null;

    }

    //타겟이 죽었나 확인
    void CheckDeadTarget(PlayerController pc)
    {
        if(pc == _target)       
            _target = null; 
        

    }
}

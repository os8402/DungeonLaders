using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : BaseController
{

    private ChasePlayerCam _cam;
    public ChasePlayerCam Cam { get { return _cam; }}

    private bool _moveKeyPressed = false;
    private bool _mouseKeyPressed = false;

    protected override void Init()
    {
        base.Init();

        Transform camera = Camera.main.transform;
        camera.parent = transform;
        camera.position = new Vector3(0, 1, -10);
        _cam = camera.GetComponent<ChasePlayerCam>();
        _cam.Init();

        Managers.Input.keyMoveEvent -= UpdateMoveInput;
        Managers.Input.keyMoveEvent += UpdateMoveInput;
        Managers.Input.KeyIdleEvent -= UpdateIdle;
        Managers.Input.KeyIdleEvent += UpdateIdle;

        //무기별 스킬 등록

        WEAPONS = Weapons.Sword;
  
        if (WEAPONS != Weapons.Empty)
        {
            weapon = Util.FindChild<BaseWeapon>(gameObject, "Sword", true);
            _skillEvent -= weapon.SkillEvent;
            _skillEvent += weapon.SkillEvent;
        }

    }
 


    void Start()
    {
        Init();
    }
    void Update()
    {
        UpdateRotation();
    }

    protected override void UpdateRotation()
    {
        Quaternion q = Util.RotateDir2D(transform.position, _cam.MousePos, true);

        if (q.z > Quaternion.identity.z) // 오른쪽
        {
            Dir = 1;
        }
        else if (q.z < Quaternion.identity.z)// 왼쪽
        {
            Dir = -1;
        }

        else return;

    }

    protected override void UpdateMoving()
    {
        if (_moveKeyPressed == false)
            return;

        Vector3 destPos  = _grid.CellToWorld(Pos) + new Vector3(0.5f , 0.5f);
        Vector3 moveDir = destPos - transform.position;

        float dist = moveDir.magnitude;

        if(dist < Speed * Time.deltaTime)
        {
            transform.position = destPos;
            _moveKeyPressed = false;


        }
        else
        {
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
           
        }
    }


    void UpdateMoveInput()
    {
        if(_moveKeyPressed == false)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3Int destPos = Pos;

            destPos += new Vector3Int((int)h, (int)v, 0);
            Debug.Log(destPos);

            if (Managers.Map.CanGo(destPos))
            {
                Pos = destPos;
                _moveKeyPressed = true;
            }
        }

        UpdateMoving();

        if (_coSkill == null && Input.GetMouseButtonDown(0))
        {
            if (CREATURE_STATE == CreatureState.Skill)
                return;

            // 스킬 공격 
            _coSkill = StartCoroutine("CoSkillAttack", 0.2f);      
        }
    }

     IEnumerator CoSkillAttack(float time)
    {
        CREATURE_STATE = CreatureState.Skill;
        _skillEvent?.Invoke();
        
        yield return new WaitForSeconds(time);
        _coSkill = null; 
    }

    
}

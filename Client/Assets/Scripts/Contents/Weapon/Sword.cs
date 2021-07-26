﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Sword : BaseWeapon
{
    //TODO : 검은 공격할 때 방향에 따라 Sorting Order도 적용
    private int _swordDir = 0;
    public Transform temp_target;

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



    // 검은 자신을 기준으로 상 or 하 or 좌 or 우  중에서 
    // 2방향을 지정하고 
    // 그 후 2방향을 기준으로 대각선도 검사함  [즉 3방향] + [자기자신의 위치도 포함합니다]
    // 검 종류마다 공격 범위가 다를 것을 고려해 간단한 BFS로 구현
    //  
    //   me ㅁ      ㅁ ㅁ
    //   ㅁ ㅁ      ㅁ me
    //   이런 느낌으로 구현

    protected override List<Vector3Int> GetAttackRange(Vector3Int cellPos , int range)
    {
        List<Vector3Int> attack_pos_list = new List<Vector3Int>();

        int X, Y;

         X = (cellPos.x > 0 ? -1 : 1);
         Y = (cellPos.y < 0 ? 1 : -1);
           
        int[] dy = { 0, Y, Y };
        int[] dx = { X, 0, X };

        Dictionary<Vector3Int, bool> visited = new Dictionary<Vector3Int, bool>();
        Queue<Vector3Int> q = new Queue<Vector3Int>();

        if (Managers.Map.OutOfMap(cellPos) == false)
            return null;

        attack_pos_list.Add(cellPos);
        visited[cellPos] = true;

        q.Enqueue(cellPos);
       

        while (q.Count > 0)
        {
            Vector3Int cur = q.Dequeue();

            for (int i = 0; i < dx.Length; i++)
            {
                int ny = cur.y + dy[i];
                int nx = cur.x + dx[i];
                Vector3Int nextPos = new Vector3Int(nx, ny, 0);

                //이미 방문했다면 넘김
                if (visited.ContainsKey(nextPos))
                    continue;

                //공격 범위를 벗어났는지 체크. 
                if (Mathf.Abs(ny - cellPos.y) > range)
                    continue;
                if (Mathf.Abs(nx - cellPos.x) > range)
                    continue;
         
                q.Enqueue(nextPos);
                visited[nextPos] = true;

                attack_pos_list.Add(nextPos);
            }
        }


        return attack_pos_list;
    }

 
    protected override void UpdateRotation()
    {

        if (_owner.Dir == -1)
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

    public override void SkillEvent()
    {
        SwordDir = (1 - SwordDir);  // -1 연산 

        GameObject go = Managers.Resource.Instantiate("Effect/Sword/Sword_Eff_001");
        EffectController ec = go.GetComponent<EffectController>();

        Vector3 moveDir = _targetPos - transform.position;
        Quaternion rot = Util.LookAt2D(_targetPos, transform.position, FacingDirection.LEFT);

        ec.transform.parent = _owner.transform;
        //소유자 등록 [누가 공격했는지 전달해줘야 함 ] 
        ec.Owner = _owner;

        // 실제 좌표 
        ec.Pos = _owner.Pos;


        //대각 4방향만 구현했음
        int dirX = (moveDir.normalized.x > 0 ? 1 : -1) * _attackRange;
        int dirY = (moveDir.normalized.y > 0 ? 1 : -1) * _attackRange;

        List<Vector3Int> attackList = GetAttackRange(new Vector3Int(dirX, dirY, 0), _attackRange);
        ec.AttackList = attackList;

        //보여주기용 좌표
        ec.transform.localPosition = new Vector2(dirX * 0.5f, dirY * 0.5f);
        ec.transform.localRotation = rot;


    }


}

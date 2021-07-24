using System.Collections;
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

    protected override List<Vector3Int> GetAttackRange(Vector3Int cellPos ,int dirX, int dirY, int range)
    {
        List<Vector3Int> attack_pos_list = new List<Vector3Int>();

        int X, Y;

         X = (dirX > 0 ? -1 : 1);
         Y = (dirY > 0 ? 1 : -1);
           
        int[] dy = { 0, Y, Y };
        int[] dx = { X, 0, X };

        bool[,] visited = new bool[Managers.Map.SizeY, Managers.Map.SizeX];
        Queue<Vector3Int> q = new Queue<Vector3Int>();

        if (Managers.Map.OutOfMap(cellPos) == false)
            return null;

        int mapX = cellPos.x - Managers.Map.MinX;
        int mapY = Managers.Map.MaxY - cellPos.y;
   
        attack_pos_list.Add(new Vector3Int(mapX , mapY , 0));

        q.Enqueue(new Vector3Int(mapX, mapY, 0));
        visited[mapY, mapX] = true; 

        while (q.Count > 0)
        {
            Vector3Int deq = q.Dequeue();

            for (int i = 0; i < dx.Length; i++)
            {
                int ny = deq.y + dy[i];
                int nx = deq.x + dx[i];

                //이미 방문했다면 넘김
                if (visited[ny, nx])
                    continue;

                //공격 범위를 벗어났는지 체크. 
                if (Mathf.Abs(ny - mapY) > range)
                    continue;
                if (Mathf.Abs(nx - mapX) > range)
                    continue;
         
                Vector3Int nextPos = new Vector3Int(nx, ny, 0); 
                q.Enqueue(nextPos);
                visited[ny, nx] = true;

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

        //대각 4방향만 구현했음
        int dirX = (moveDir.normalized.x > 0 ? 1 : -1) * _attackRange;
        int dirY = (moveDir.normalized.y > 0 ? 1 : -1) * _attackRange;

        //플레이어에게 종속되어 있어서 로컬 포지션
        //자연스럽게 보여주기 위한 용도라 계산할 좌표는 Controller 안에 있는 Pos가 처리해야합니다. 
        ec.transform.localPosition = new Vector2(dirX , dirY);
        //소수점 전부 내려야 정확합니다. [월드 포지션 보내야함]
        ec.Pos = Vector3Int.FloorToInt(ec.transform.position);
        //소유자 등록 [주인은 못 때리도록^^ ] 
        ec.Owner = _owner;

        ec.transform.localPosition = new Vector2(dirX * 0.5f, dirY * 0.5f);
        ec.transform.localRotation = rot;


        List<Vector3Int> attackList = GetAttackRange(ec.Pos, dirX, dirY, _attackRange);
        ec.AttackList = attackList;
        
    }


}

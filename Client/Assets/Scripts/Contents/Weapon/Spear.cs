using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Spear : BaseWeapon
{
    Coroutine _coMove = null;
    bool _isRot = true; 

    void Awake()
    {
        Init();
    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 1;
    }
    
    //창은 바라보는 방향 + attackRange만 계산해주면 됩니다.
    //     ㅁ           
    // ㅁ  me  ㅁ
    //     ㅁ      

    protected override List<Vector3Int> GetAttackRange(Vector3Int cellPos, int range)
    {
        List<Vector3Int> attackList = new List<Vector3Int>();

        if (Managers.Map.OutOfMap(cellPos) == false)
            return null;

        for (int i = 1; i <= range; i++)       
            attackList.Add(new Vector3Int(cellPos.x * i, cellPos.y * i, 0));
        

        return attackList;
    }

    protected override void UpdateRotation()
    {
        if (_isRot == false)
            return;

        _q = Util.LookAt2D(transform.position, _targetPos , FacingDirection.UP);
        transform.rotation = _q;

    }
    public override void SkillEvent()
    {
 

        GameObject go = Managers.Resource.Instantiate("Effect/Spear/Spear_Eff_001");
        EffectController ec = go.GetComponent<EffectController>();
        ec.transform.parent = transform.parent;

        Vector3 moveDir = _targetPos - _owner.transform.position;
        Quaternion rot = Util.LookAt2D(_targetPos, transform.position, FacingDirection.LEFT);

        //실제 좌표
        ec.Pos = _owner.Pos;

        //소유자 등록 [누가 공격했는지 전달해줘야 함 ] 
        ec.Owner = _owner;

        Vector3Int attackPos = Vector3Int.RoundToInt(moveDir.normalized);

        _coMove = StartCoroutine(CoMoveSpear(ec, attackPos));

        //공격 범위
        List<Vector3Int> attackList = GetAttackRange(attackPos, _attackRange);
        ec.AttackList = attackList;

        //보여주기용 좌표 
        ec.transform.localPosition = moveDir.normalized * 0.2f;
        ec.transform.localRotation = rot;


    }


    IEnumerator CoMoveSpear(EffectController ec , Vector3Int destPos,  float time = 0.2f)
    {     
        Vector3 newPos = new Vector3(destPos.x * 0.2f, destPos.y * 0.2f, 0);
        transform.localPosition = newPos;      
        yield return new WaitForSeconds(time);
        transform.localPosition = -newPos;
        _coMove = null;

    }
  
}

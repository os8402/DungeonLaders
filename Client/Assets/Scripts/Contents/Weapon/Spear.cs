using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Spear : BaseWeapon
{
    Coroutine _coMove = null; 

    void Awake()
    {
        Init();
    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 3;
    }
    protected override List<Vector3Int> GetAttackRange(Vector3Int cellPos, int dirX, int dirY, int range)
    {
        List<Vector3Int> attackList = new List<Vector3Int>();



        return attackList;
    }

    protected override void UpdateRotation()
    {
        _q = Util.LookAt2D(transform.position, _targetPos , FacingDirection.UP);
        transform.rotation = _q;

    }
    public override void SkillEvent()
    {


        GameObject go = Managers.Resource.Instantiate("Effect/Spear/Spear_Eff_001");
        go.SetActive(false); 
        EffectController ec = go.GetComponent<EffectController>();

        Vector3 moveDir = _targetPos - transform.position;
        Quaternion rot = Util.LookAt2D(_targetPos, transform.position, FacingDirection.LEFT);

        
        //실제 좌표
        ec.Pos = Vector3Int.RoundToInt(moveDir.normalized);

        //소유자 등록 [주인은 못 때리도록^^ ] 
        ec.Owner = _owner;
        // ec.transform.parent = _owner.transform;
        ec.transform.parent = transform.parent;


        _coMove = StartCoroutine(CoMoveSpear(ec));
        //보여주기용 좌표
        ec.transform.localPosition =  moveDir.normalized * 0.8f;
        ec.transform.localRotation = rot;

        List<Vector3Int> attackList = GetAttackRange(ec.Pos, ec.Pos.x, ec.Pos.y, _attackRange);
        ec.AttackList = attackList;
    }


    IEnumerator CoMoveSpear(EffectController ec ,  float time = 0.1f)
    {
        Vector3 newPos = new Vector3(ec.Pos.x * 0.2f, ec.Pos.y * 0.2f, 0);
        transform.localPosition = -newPos;
        
        yield return new WaitForSeconds(time);
        ec.gameObject.SetActive(true);
        transform.localPosition = newPos;
        _coMove = null; 
    }
  
}

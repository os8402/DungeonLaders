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

    protected override int GetDirFromNormal(float num )
    {

        int dir = 0; 

        if (num >= 0.05f)
            dir = 1;
        else if (num <= -0.05f)
            dir = -1;

        return dir; 

    }

    public override void SkillEvent()
    {

        base.SkillEvent();
        //보여주기용 좌표 
        _coMove = StartCoroutine(CoMoveSpear(_ec, base._attackPos));
        _ec.transform.localPosition = _moveDir.normalized * 0.2f;
        _ec.transform.localRotation = _rot;
        _ec = null;

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

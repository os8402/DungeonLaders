using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : BaseController
{

    Coroutine _coHit = null;
    public CreatureController Owner { get; set; }
    public List<AttackPos> AttackList { get; set; }

    public float destroyTime = 0.5f;


    private void Awake()
    {
        _ignoreCollision = true;
    }

    void Start()
    {
        Init();
     
    }

    protected override void Init()
    {
        //_ignoreCollision = true;
        UpdateIdle();
        
    }

    protected override void MoveToNextPos() { }
  
    //이펙트는 Idle에 다 넣고 처리
    protected override void UpdateIdle()
    {   
       if(_coHit == null)
        {
            StartCoroutine("CoHitCreature", destroyTime);
        }
    }

    protected override void UpdateRotation() { }

    protected override void UpdateController() { }

    void AttackObject()
    {
        if (AttackList == null || Owner == null)
            return;

        foreach (AttackPos pos in AttackList)
        {
            Vector3Int attkPos = new Vector3Int(pos.AttkPosX, pos.AttkPosY , 0);
            Vector3Int destPos = CellPos + attkPos; 
            //맵을 벗어낫는지 attkPos
            if (Managers.Map.OutOfMap(destPos))
                continue;

            Managers.Map.VisibleCellEffect(destPos, Owner);

        }
    }

    IEnumerator CoHitCreature(float time)
    {

        AttackObject();
        yield return new WaitForSeconds(time);
        Managers.Resource.Destroy(gameObject);
    }

}

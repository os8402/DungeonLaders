using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : BaseController
{

    Coroutine _coHit = null;
    public CreatureController Owner { get; set; }
    public List<AttackPos> AttackList { get; set; } = new List<AttackPos>();

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
        if (AttackList == null || Owner == null || Owner.MyWeapon.GetType() == typeof(Bow))
            return;

        foreach (AttackPos pos in AttackList)
        {
            Vector3Int attkPos = new Vector3Int(pos.AttkPosX, pos.AttkPosY , 0);
            Vector3Int destPos;
            if (Owner.MyWeapon.GetType() != typeof(Staff))
                destPos = CellPos + attkPos;
            else
                destPos = attkPos;

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

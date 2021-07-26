using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : BaseController
{

    Coroutine _coHit = null;
    public CreatureController Owner { get; set; }
    public List<Vector3Int> AttackList { get; set; }


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
            StartCoroutine("CoHitCreature", 0.5f);
        }
    }

    protected override void UpdateRotation() { }

    protected override void UpdateController() { }

    void AttackObject()
    {
        if (AttackList == null)
            return;

        foreach (Vector3Int pos in AttackList)
        {
            
            GameObject obj = Managers.Map.Objects[pos.y , pos.x];

            if (obj == null || Owner.gameObject == obj)
                continue;

            CreatureController cc = obj.GetComponent<CreatureController>();

            Debug.Log($"Attak : {obj.name} , ({pos}) , Hp : {cc.Hp} ");
            cc.OnDamaged(Owner.gameObject, 10);

        }
    }

    IEnumerator CoHitCreature(float time)
    {

        AttackObject();
        yield return new WaitForSeconds(time);
        Managers.Resource.Destroy(gameObject);
    }

}

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

            int mapX = (Pos.x +  pos.x) - Managers.Map.MinX;
            int mapY = Managers.Map.MaxY - (Pos.y + pos.y);

            GameObject go = Managers.Map.Objects[mapY, mapX];
      
            if (go == null || Owner.gameObject == go)
                continue;

            CreatureController cc = go.GetComponent<CreatureController>();
            Debug.Log($"{Owner.TeamId} vs {cc.TeamId}");

            if (Owner.TeamId == cc.TeamId)
                continue;

            Debug.Log($"Attak : {go.name} , ({pos}) , Hp : {cc.Hp} ");
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

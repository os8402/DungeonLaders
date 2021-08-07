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

            Vector3Int destPos = CellPos + pos; 
            //맵을 벗어낫는지 체크
            if (Managers.Map.OutOfMap(destPos))
                continue;

            int mapX = (CellPos.x + pos.x) - Managers.Map.MinX;
            int mapY = Managers.Map.MaxY - (CellPos.y + pos.y);

            {
                //개발단계에서 공격범위 확인 용
                //서버에서 패킷보낼 때 잘못 갈 수도 있으므로 대비하기 위함
                SpriteRenderer seeAttack = Managers.Resource.Instantiate("Effect/Common/AttackRange_Eff").GetComponent<SpriteRenderer>();
                seeAttack.transform.position = new Vector3(CellPos.x + pos.x, CellPos.y + pos.y) + (Vector3.one * 0.5f);

                if (Owner.GetType() == typeof(PlayerController))
                {
                    seeAttack.color = Color.yellow;
                    seeAttack.sortingOrder += 1;
                }
                                    
                else
                    seeAttack.color = Color.blue;

            }


       

            GameObject go = Managers.Map.Objects[mapY, mapX];
      
            if (go == null || Owner.gameObject == go)
                continue;

            CreatureController cc = go.GetComponent<CreatureController>();
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

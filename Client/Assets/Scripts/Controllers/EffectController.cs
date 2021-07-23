using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : BaseController
{

    Coroutine _coHit = null;
    public CreatureController Owner { get; set; }

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
            StartCoroutine("CoHitCreature", 1.5f);
        }
    }

    protected override void UpdateRotation() { }

    protected override void UpdateController() { }


    IEnumerator CoHitCreature(float time)
    {


        Dictionary<int, GameObject> list = Managers.Object.FindHitCreature(Pos, 2);
        foreach (GameObject obj in list.Values)
        {
            if (obj == gameObject || Owner.gameObject == obj)
                continue;

            Vector3 targetPos = obj.transform.position.normalized;

            float dot = Vector3.Dot(targetPos, transform.up);

            Debug.Log($"{obj.name}_({dot})");
            if (dot >= 0f)
            {
                Debug.Log("타격 성공");
            }

        }
           

        yield return new WaitForSeconds(time);
        Managers.Resource.Destroy(gameObject);
    }

}

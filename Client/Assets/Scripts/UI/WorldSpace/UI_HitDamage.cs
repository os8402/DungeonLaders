using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HitDamage : UI_Base
{

    public Color[] colors;  

    enum Texts
    {
        DamageText
    }

    public CreatureController Creature { get; set; }

    public int Damage { get; set; }

    public override void Init()
    {
        Bind<Text>(typeof(Texts));

    }
    public void RefreshUI()
    {
        
        transform.position = Creature.transform.position;


        GetText((int)Texts.DamageText).color = colors[Creature.HitLayer % 10];
        GetText((int)Texts.DamageText).text = $"{Damage}";

        Creature.HitLayer++;
    }

    float _speed = 1.3f;
    Coroutine _coTick = null; 

    void Update()
    {
        transform.position += (Vector3.up * _speed * Time.deltaTime);

        if(_coTick == null)
        {
            _coTick = StartCoroutine(CoCheckTick());
        }
    }

    IEnumerator CoCheckTick()
    {
        yield return new WaitForSeconds(.5f);
        Creature.HitLayer--;
        Managers.Resource.Destroy(gameObject);

    }
}

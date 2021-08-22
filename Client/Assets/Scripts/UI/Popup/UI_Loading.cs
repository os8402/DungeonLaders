using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : UI_Popup
{
    enum Texts
    {
        LodingText
    }

    public override void Init()
    {
        base.Init();
        Bind<Text>(typeof(Texts));

    }


    string[] _ment = { "", ".","..","...","...." };
    int _idx = 0;
    Coroutine _coTick = null;

    void Update()
    {
     
        if(_coTick == null)
            _coTick = StartCoroutine("CheckTickLoading");
         
    }

    IEnumerator CheckTickLoading()
    {

        GetText((int)Texts.LodingText).text = "Loading" + _ment[_idx++];

        if (_idx == _ment.Length)
            _idx = 0; 

        yield return new WaitForSeconds(.5f);
        _coTick = null; 

    }
}

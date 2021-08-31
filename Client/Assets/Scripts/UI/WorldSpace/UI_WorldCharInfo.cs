using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WorldCharInfo : UI_Base
{
    [SerializeField]
    private Text _charInfoText = null;
    

    public override void Init()
    {
        
    }

    public void RefreshUI(int lv , string creatureName )
    {
        _charInfoText.text = $"LV {lv} {creatureName}"; 
       
    }

}

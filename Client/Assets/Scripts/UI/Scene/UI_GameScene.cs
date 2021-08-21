using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_Inventory InvenUI { get; private set; }
    public UI_Stat StatUI { get; private set; }
    public UI_Status StatusUI { get; private set; }
    public UI_Coin CoinUI { get; private set; }
    public UI_Passive PassiveUI { get; private set; }

    public override void Init()
    {
        base.Init();

        InvenUI = GetComponentInChildren<UI_Inventory>();
        StatUI = GetComponentInChildren<UI_Stat>();
        StatusUI = GetComponentInChildren<UI_Status>();
        CoinUI = GetComponentInChildren<UI_Coin>();
        PassiveUI = GetComponentInChildren<UI_Passive>();

        InvenUI.gameObject.SetActive(false);
        StatUI.gameObject.SetActive(false);
        StatusUI.gameObject.SetActive(false);
        CoinUI.gameObject.SetActive(false);
        PassiveUI.gameObject.SetActive(false); 

    }

 

}

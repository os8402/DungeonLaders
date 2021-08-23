using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    enum Buttons
    {
        Exit_Btn_All
    }

    public UI_Inventory InvenUI { get;  set; }
    public UI_Stat StatUI { get; private set; }
    public UI_Status StatusUI { get; private set; }
    public UI_Coin CoinUI { get; private set; }
    public UI_Passive PassiveUI { get; private set; }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        BindEvent(GetButton((int)Buttons.Exit_Btn_All).gameObject, (e) => 
        {
            InvenUI.gameObject.SetActive(false);
            StatUI.gameObject.SetActive(false);
        });

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

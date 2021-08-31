using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectServerItem : UI_Base
{

    [SerializeField]
    public Sprite[] _selected_spr_list = null;

    [SerializeField]
    public Sprite[] _icon_spr_list = null;

    [SerializeField]
    public Color[] _busy_color_list = null;

    public int? Index { get; set; }

    public UI_SelectServerPopup PopupUI { get; set; } 

    public ServerInfo Info { get; set; }

    enum Images
    {
        ItemIcon,
        SelectServer_Bg,
     
    }
    enum Texts
    {
        ItemName,
        ItemBusy
    }
    

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        GetImage((int)Images.SelectServer_Bg).gameObject.BindEvent(OnClickEvent);

    }


    public void RefreshUI()
    {
        if (Info == null)
            return;


        GetText((int)Texts.ItemName).text = Info.Name;
        GetImage((int)Images.SelectServer_Bg).sprite = _selected_spr_list[0];


        // 0 ~ 40 쾌적
        // 41 ~ 70 보통
        // 71 ~ 99 혼잡
        // 100 포화 

        List<int> crowdedScore = new List<int>() { 41, 71, 100 };
        List<string> crowdedText = new List<string>() { "쾌적", "보통", "혼잡" };

        string busyStr = "포화";
        int idx = 0; 

        for (idx = 0; idx < crowdedScore.Count; idx++)
        {
            if (Info.BusyScore < crowdedScore[idx])
            {
                busyStr = crowdedText[idx];
                break;
            }
        }

        GetText((int)Texts.ItemBusy).text = busyStr;
        GetText((int)Texts.ItemBusy).color = _busy_color_list[idx];

        if (Index == null)
            return; 

        GetImage((int)Images.ItemIcon).sprite = _icon_spr_list[Index.Value];
    }

    void OnClickEvent(PointerEventData evt)
    {
 
        if (Index == null)
            return;

  
        PopupUI.RefreshUI();
        PopupUI.Selected[Index.Value] = true;
        GetImage((int)Images.SelectServer_Bg).sprite = _selected_spr_list[1];

        PopupUI.SetServerInfo(Info);

    }

}

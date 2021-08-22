using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectNpc : MonoBehaviour
{

    private OutlineEffect _outline;
    public LobbyPlayerInfo PlayerInfo { get; set; }

    public string JobName { get; set; }

    void Start()
    {
        _outline = GetComponent<OutlineEffect>();
        _outline.enabled = false; 
    }
    private void OnMouseEnter()
    {
        _outline.enabled = true;

    }
    private void OnMouseExit()
    {
        _outline.enabled = false;

    }


    private void OnMouseDown()
    {
        Managers.UI.CloseAllPopupUI();
        UI_SelectInfo selectInfo = Managers.UI.ShowPopupUI<UI_SelectInfo>();
        selectInfo.RefreshUI(this);

    }

}

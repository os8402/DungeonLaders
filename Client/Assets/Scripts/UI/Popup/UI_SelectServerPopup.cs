using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectServerPopup : UI_Popup
{
    [SerializeField]
    public Sprite[] _icon_spr_list = null;

    public List<UI_SelectServerItem> Items { get; } = new List<UI_SelectServerItem>();


    public bool[] Selected { get; set; }
    public List<string> ServerNames { get; set; } = new List<string>();

    enum GameObjects
    {
        ServerInfo_Go,
        BusyScoreSlider,
        EnterBtn
    }

    enum Images
    {
        Icon_SelectServer,
    }
    enum Texts
    {
        Text_SelectServer,
        BusyScoreText
    }
 

    public override void Init()
    {
        base.Init();
    }

    public void SetServers(List<ServerInfo> servers)
    {
        Items.Clear();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        GetObject((int)GameObjects.ServerInfo_Go).SetActive(false);

        GameObject grid = GetComponentInChildren<GridLayoutGroup>().gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);


        for (int i = 0; i < servers.Count; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Popup/UI_SelectServerItem", grid.transform);
            UI_SelectServerItem item = go.GetOrAddComponent<UI_SelectServerItem>();
            Items.Add(item);

            item.Info = servers[i];
            item.Index = i;
            ServerNames.Add(servers[i].Name);
            item.PopupUI = this;
        }


        RefreshUI();

    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        Selected = new bool[Items.Count];

        foreach (var item in Items)
        {
            item.RefreshUI();
        }

    }

    //들어갈 서버의 상세정보
    public void SetServerInfo(ServerInfo info)
    {
        if (Selected.Length == 0)
            return;

        int idx = 0;

        foreach (bool s in Selected)
        {
            if (s)
                break;

            idx++;
        }

        GetObject((int)GameObjects.ServerInfo_Go).SetActive(true);

        GetImage((int)Images.Icon_SelectServer).sprite = _icon_spr_list[idx];
        GetText((int)Texts.Text_SelectServer).text = ServerNames[idx];

        Slider slider = GetObject((int)GameObjects.BusyScoreSlider).GetComponent<Slider>();

        slider.value = info.BusyScore;
        GetText((int)Texts.BusyScoreText).text = $"{info.BusyScore}/{100}";


        //겹칠 수도 있어서 이벤트 전부 제거
        GetObject((int)GameObjects.EnterBtn).BindRemoveAllEvent();

        GetObject((int)GameObjects.EnterBtn).BindEvent((e) =>
        {

            //포화
            if (info.BusyScore == 100)
                return;

            UI_Loading loading = Managers.UI.ShowPopupUI<UI_Loading>();

            Managers.Network.ConnectToGame(info);
            Managers.Scene.LoadScene(Define.Scene.Lobby);

        });

    }
}

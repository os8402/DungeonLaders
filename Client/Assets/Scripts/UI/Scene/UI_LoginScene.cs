using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
   enum Inputs
    {
        Account,
        Password
    }
    enum Images
    {
        CreateBtn,
        LoginBtn
    }

    public override void Init()
    {
        base.Init();

        Bind<InputField>(typeof(Inputs)); 
        Bind<Image>(typeof(Images));

        GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
        GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
    }


    UI_Loading _loading;


    public void OnClickCreateButton(PointerEventData evt)
    {
        string account = GetInput((int)Inputs.Account).text;
        string password = GetInput((int)Inputs.Password).text;

        _loading = Managers.UI.ShowPopupUI<UI_Loading>();

        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password,
        };

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
        {
            Debug.Log(res.CreateOk);
            GetInput((int)Inputs.Account).text = "";
            GetInput((int)Inputs.Password).text = "";

            Managers.UI.ClosePopupUI(_loading);
        });

    

    }
    public void OnClickLoginButton(PointerEventData evt)
    {
        Debug.Log("OnClickLogin");

        string account = GetInput((int)Inputs.Account).text;
        string password = GetInput((int)Inputs.Password).text;

        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password,
        };

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
        {
            Debug.Log(res.LoginOk);

         //   UI_Loading loading = Managers.UI.ShowPopupUI<UI_Loading>();

            GetInput((int)Inputs.Account).text = "";
            GetInput((int)Inputs.Password).text = "";

       
            if (res.LoginOk)
            {
                Managers.Network.AccountId = res.AccountDbId;
                Managers.Network.Token = res.Token; 

                UI_SelectServerPopup serverPopup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                serverPopup.SetServers(res.ServerList);
            }     

        });

      
    }



}

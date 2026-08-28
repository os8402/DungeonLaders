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
    enum Texts
    {
        // 프리팹에 있었지만 아무도 쓰지 않던 텍스트.
        // 계정 생성/로그인 결과를 여기에 표시한다.
        LoginInfoText
    }

    public override void Init()
    {
        base.Init();

        Bind<InputField>(typeof(Inputs));
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
        GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);

        SetInfo("");
    }

    UI_Loading _loading;

    public void OnClickCreateButton(PointerEventData evt)
    {
        string account = GetInput((int)Inputs.Account).text;
        string password = GetInput((int)Inputs.Password).text;

        if (!ValidateInput(account, password))
            return;

        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password,
        };

        SetInfo("계정을 만드는 중...");
        ShowLoading();

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet,
            (res) =>
            {
                HideLoading();

                if (res.CreateOk)
                {
                    ClearInputs();
                    SetInfo($"'{account}' 계정을 만들었습니다. 로그인하세요.");
                }
                else
                {
                    // 서버는 중복 이름일 때 CreateOk=false 를 돌려준다.
                    SetInfo("이미 존재하는 계정 이름입니다.", isError: true);
                }
            },
            (error) =>
            {
                HideLoading();
                SetInfo(error, isError: true);
            });
    }

    public void OnClickLoginButton(PointerEventData evt)
    {
        string account = GetInput((int)Inputs.Account).text;
        string password = GetInput((int)Inputs.Password).text;

        if (!ValidateInput(account, password))
            return;

        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password,
        };

        SetInfo("로그인 중...");
        ShowLoading();

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet,
            (res) =>
            {
                HideLoading();

                if (res.LoginOk == false)
                {
                    SetInfo("계정 이름 또는 비밀번호가 올바르지 않습니다.", isError: true);
                    return;
                }

                ClearInputs();

                Managers.Network.AccountId = res.AccountDbId;
                Managers.Network.Token = res.Token;

                // 서버 목록이 비어 있으면 채널을 고를 수 없다.
                // 게임 서버를 하나도 켜지 않은 흔한 상황이므로 원인을 알려준다.
                if (res.ServerList == null || res.ServerList.Count == 0)
                {
                    SetInfo("접속 가능한 게임 서버가 없습니다. GameServer 를 실행하세요.", isError: true);
                    return;
                }

                SetInfo("");
                UI_SelectServerPopup serverPopup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                serverPopup.SetServers(res.ServerList);
            },
            (error) =>
            {
                HideLoading();
                SetInfo(error, isError: true);
            });
    }

    bool ValidateInput(string account, string password)
    {
        // 원래는 검증이 없어서 빈 문자열 계정이 그대로 DB 에 들어갔다.
        if (string.IsNullOrWhiteSpace(account))
        {
            SetInfo("계정 이름을 입력하세요.", isError: true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            SetInfo("비밀번호를 입력하세요.", isError: true);
            return false;
        }

        return true;
    }

    void SetInfo(string message, bool isError = false)
    {
        Text info = GetText((int)Texts.LoginInfoText);
        if (info == null)
            return;

        info.text = message;
        info.color = isError ? new Color(0.9f, 0.3f, 0.3f) : Color.white;
    }

    void ClearInputs()
    {
        GetInput((int)Inputs.Account).text = "";
        GetInput((int)Inputs.Password).text = "";
    }

    void ShowLoading()
    {
        if (_loading == null)
            _loading = Managers.UI.ShowPopupUI<UI_Loading>();
    }

    void HideLoading()
    {
        if (_loading == null)
            return;

        Managers.UI.ClosePopupUI(_loading);
        _loading = null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    public string BaseUrl { get; set; } = "https://localhost:5001/api";

    /// <summary>
    /// 계정 서버에 POST 요청을 보낸다.
    /// </summary>
    /// <param name="onSuccess">2xx 응답 + 역직렬화 성공 시 호출</param>
    /// <param name="onError">
    /// 통신 실패 / 서버 오류 / 응답 파싱 실패 시 호출된다.
    /// 원래는 실패하면 <b>아무 콜백도 불리지 않아서</b> 호출부의 로딩 팝업이
    /// 영원히 닫히지 않고 화면이 멈춘 것처럼 보였다. 반드시 둘 중 하나는 호출된다.
    /// </param>
    public void SendPostRequest<T>(string url, object obj, Action<T> onSuccess, Action<string> onError = null)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, "POST", obj, onSuccess, onError));
    }

    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> onSuccess, Action<string> onError)
    {
        string sendUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;
        if (obj != null)
        {
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            // Unity 2020.2+ : isNetworkError / isHttpError 는 삭제됨 -> UnityWebRequest.result
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                string message = DescribeFailure(uwr);
                Debug.LogWarning($"[Web] {method} {sendUrl} 실패 : {message}");
                onError?.Invoke(message);
                yield break;
            }

            T resObj;
            try
            {
                resObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(uwr.downloadHandler.text);
            }
            catch (Exception e)
            {
                string message = "서버 응답을 해석하지 못했습니다.";
                Debug.LogWarning($"[Web] {sendUrl} 응답 파싱 실패 : {e.Message}\n{uwr.downloadHandler.text}");
                onError?.Invoke(message);
                yield break;
            }

            onSuccess?.Invoke(resObj);
        }
    }

    /// <summary>실패 원인을 사용자에게 보여줄 수 있는 문장으로 바꾼다.</summary>
    static string DescribeFailure(UnityWebRequest uwr)
    {
        switch (uwr.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                return "계정 서버에 연결할 수 없습니다. AccountServer 가 실행 중인지 확인하세요.";
            case UnityWebRequest.Result.ProtocolError:
                return $"서버가 오류를 반환했습니다. (HTTP {uwr.responseCode})";
            case UnityWebRequest.Result.DataProcessingError:
                return "서버 응답을 처리하지 못했습니다.";
            default:
                return uwr.error;
        }
    }
}

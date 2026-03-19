using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService : MonoBehaviour
{
    public IEnumerator Register(string username, string password, Action<bool, string> callback)
    {
        string url = $"{ApiConfig.BaseUrl}/api/usuarios";

        RegisterRequest reqData = new RegisterRequest
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(reqData);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            callback?.Invoke(true, request.downloadHandler.text);
        else
            callback?.Invoke(false, request.downloadHandler.text);
    }

    public IEnumerator Login(string username, string password, Action<bool, string> callback)
    {
        string url = $"{ApiConfig.BaseUrl}/api/auth/login";

        LoginRequest reqData = new LoginRequest
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(reqData);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            callback?.Invoke(true, request.downloadHandler.text);
        else
            callback?.Invoke(false, request.downloadHandler.text);
    }

    public IEnumerator GetProfile(string username, string token, Action<bool, string> callback)
    {
        string url = $"{ApiConfig.BaseUrl}/api/usuarios?username={UnityWebRequest.EscapeURL(username)}";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            callback?.Invoke(true, request.downloadHandler.text);
        else
            callback?.Invoke(false, request.downloadHandler.text);
    }

    public IEnumerator UpdateScore(string username, string token, int newScore, Action<bool, string> callback)
    {
        string url = $"{ApiConfig.BaseUrl}/api/usuarios";

        UpdateRequest reqData = new UpdateRequest
        {
            username = username,
            data = new UserData { score = newScore }
        };

        string json = JsonUtility.ToJson(reqData);

        using UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            callback?.Invoke(true, request.downloadHandler.text);
        else
            callback?.Invoke(false, request.downloadHandler.text);
    }

    public IEnumerator GetUsers(string token, int limit, int skip, bool sort, Action<bool, string> callback)
    {
        string url = $"{ApiConfig.BaseUrl}/api/usuarios?limit={limit}&skip={skip}&sort={sort.ToString().ToLower()}";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            callback?.Invoke(true, request.downloadHandler.text);
        else
            callback?.Invoke(false, request.downloadHandler.text);
    }
}
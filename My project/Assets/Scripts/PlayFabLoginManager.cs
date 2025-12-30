using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLoginManager : MonoBehaviour
{
    void Start()
    {
        Login();
    }

    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            // デバイス固有のIDをCustomIDとして使用
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true // アカウントがなければ作成する
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab ログイン成功！ プレイヤーID: " + result.PlayFabId);
    }

    void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab ログイン失敗: " + error.GenerateErrorReport());
    }
}
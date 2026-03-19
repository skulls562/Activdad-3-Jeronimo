using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public string Token { get; private set; }
    public string Username { get; private set; }

    private const string TokenKey = "AUTH_TOKEN";
    private const string UserKey = "AUTH_USER";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSession(string username, string token)
    {
        Username = username;
        Token = token;

        PlayerPrefs.SetString(UserKey, username);
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.Save();
    }

    public void LoadSession()
    {
        Username = PlayerPrefs.GetString(UserKey, "");
        Token = PlayerPrefs.GetString(TokenKey, "");
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(Token);
    }

    public void Logout()
    {
        Username = "";
        Token = "";
        PlayerPrefs.DeleteKey(UserKey);
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.Save();
    }
}
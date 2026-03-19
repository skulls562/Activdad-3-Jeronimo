using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Services")]
    public ApiService apiService;

    [Header("Panels")]
    public GameObject panelAuth;
    public GameObject panelHome;
    public GameObject panelRanking;

    [Header("Register UI")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;

    [Header("Login UI")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;

    [Header("Home UI")]
    public TMP_Text welcomeText;
    public TMP_Text scoreText;
    public TMP_InputField newScoreInput;

    [Header("Messages")]
    public TMP_Text messageText;

    [Header("Ranking UI")]
    public TMP_Text rankingText;

    private void Start()
    {
        RefreshAuthState();
    }

    public void OnRegisterButton()
    {
        string username = registerUsernameInput.text.Trim();
        string password = registerPasswordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetMessage("Completa usuario y contraseña para registrarte.");
            return;
        }

        StartCoroutine(apiService.Register(username, password, (ok, response) =>
        {
            if (ok)
                SetMessage("Registro exitoso. Ahora inicia sesión.");
            else
                SetMessage("Error en registro: " + response);
        }));
    }

    public void OnLoginButton()
    {
        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetMessage("Completa usuario y contraseña para iniciar sesión.");
            return;
        }

        StartCoroutine(apiService.Login(username, password, (ok, response) =>
        {
            Debug.Log("LOGIN ok = " + ok);
            Debug.Log("LOGIN response = " + response);

            if (ok)
            {
                LoginResponse loginRes = JsonUtility.FromJson<LoginResponse>(response);

                if (loginRes != null && !string.IsNullOrEmpty(loginRes.token))
                {
                    string safeUsername = username;

                    if (loginRes.usuario != null && !string.IsNullOrEmpty(loginRes.usuario.username))
                        safeUsername = loginRes.usuario.username;

                    AuthManager.Instance.SaveSession(safeUsername, loginRes.token);

                    SetMessage("Login exitoso.");
                    RefreshAuthState();
                    LoadProfile();
                }
                else
                {
                    SetMessage("No se pudo leer el token del servidor.");
                }
            }
            else
            {
                SetMessage("Error en login: " + response);
            }
        }));
    }

    public void OnLogoutButton()
    {
        AuthManager.Instance.Logout();
        SetMessage("Sesión cerrada.");
        RefreshAuthState();
    }

    public void OnUpdateScoreButton()
    {
        if (!AuthManager.Instance.IsAuthenticated())
        {
            SetMessage("Debes iniciar sesión.");
            RefreshAuthState();
            return;
        }

        if (!int.TryParse(newScoreInput.text, out int newScore))
        {
            SetMessage("Ingresa un score válido.");
            return;
        }

        Debug.Log("Intentando actualizar score a: " + newScore);

        StartCoroutine(apiService.UpdateScore(AuthManager.Instance.Username, AuthManager.Instance.Token, newScore, (ok, response) =>
        {
            Debug.Log("Respuesta UPDATE ok=" + ok);
            Debug.Log("Respuesta UPDATE body: " + response);

            if (ok)
            {
                SetMessage("Score actualizado.");
                LoadProfile();
            }
            else
            {
                SetMessage("Error al actualizar: " + response);
            }
        }));
    }

    public void OnShowRankingButton()
    {
        if (!AuthManager.Instance.IsAuthenticated())
        {
            SetMessage("Debes iniciar sesión.");
            return;
        }

        StartCoroutine(apiService.GetUsers(AuthManager.Instance.Token, 100, 0, true, (ok, response) =>
        {
            if (ok)
            {
                UsersListResponse usersRes = JsonUtility.FromJson<UsersListResponse>(FixJsonArray(response));

                if (usersRes != null && usersRes.usuarios != null)
                {
                    List<UserResponse> users = usersRes.usuarios;

                    users.Sort((a, b) =>
                    {
                        int scoreA = a.data != null ? a.data.score : 0;
                        int scoreB = b.data != null ? b.data.score : 0;
                        return scoreB.CompareTo(scoreA);
                    });

                    rankingText.text = "RANKING\n\n";
                    for (int i = 0; i < users.Count; i++)
                    {
                        int score = users[i].data != null ? users[i].data.score : 0;
                        rankingText.text += $"{i + 1}. {users[i].username} - {score}\n";
                    }

                    panelHome.SetActive(false);
                    panelRanking.SetActive(true);
                }
                else
                {
                    rankingText.text = "No se pudieron cargar usuarios.";
                }
            }
            else
            {
                SetMessage("Error al consultar ranking: " + response);
            }
        }));
    }

    public void OnBackFromRanking()
    {
        panelRanking.SetActive(false);
        panelHome.SetActive(true);
    }

    private void LoadProfile()
    {
        Debug.Log("USERNAME guardado en sesión = " + AuthManager.Instance.Username);
        Debug.Log("TOKEN guardado en sesión = " + AuthManager.Instance.Token);

        StartCoroutine(apiService.GetProfile(AuthManager.Instance.Username, AuthManager.Instance.Token, (ok, response) =>
        {
            Debug.Log("PROFILE ok = " + ok);
            Debug.Log("PROFILE response = " + response);

            if (ok)
            {
                ProfileResponse profile = JsonUtility.FromJson<ProfileResponse>(response);

                if (profile != null && profile.usuarios != null && profile.usuarios.Count > 0)
                {
                    UserResponse myUser = null;

                    for (int i = 0; i < profile.usuarios.Count; i++)
                    {
                        if (profile.usuarios[i].username == AuthManager.Instance.Username)
                        {
                            myUser = profile.usuarios[i];
                            break;
                        }
                    }

                    if (myUser == null)
                    {
                        myUser = profile.usuarios[0];
                    }

                    int score = (myUser.data != null) ? myUser.data.score : 0;
                    string finalUsername = !string.IsNullOrEmpty(myUser.username)
                        ? myUser.username
                        : AuthManager.Instance.Username;

                    welcomeText.text = $"Bienvenido, {finalUsername}";
                    scoreText.text = $"Score actual: {score}";
                }
                else
                {
                    welcomeText.text = $"Bienvenido, {AuthManager.Instance.Username}";
                    scoreText.text = "Score actual: 0";
                }
            }
            else
            {
                SetMessage("Error al cargar perfil: " + response);
            }
        }));
    }

    private void RefreshAuthState()
    {
        bool logged = AuthManager.Instance != null && AuthManager.Instance.IsAuthenticated();

        panelAuth.SetActive(!logged);
        panelHome.SetActive(logged);
        panelRanking.SetActive(false);

        if (logged)
            welcomeText.text = $"Bienvenido, {AuthManager.Instance.Username}";
    }

    private void SetMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }

    private string FixJsonArray(string json)
    {
        if (json.TrimStart().StartsWith("["))
            return "{\"usuarios\":" + json + "}";

        return json;
    }
}
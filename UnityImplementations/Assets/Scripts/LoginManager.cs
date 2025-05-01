using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public Button loginButton;
    public Button registerLink;
    public GameObject loadingPanel;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public string mainMenuSceneName = "Main menu";

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerLink.onClick.AddListener(OnRegisterLinkClicked);
    }

    private void OnLoginButtonClicked()
    {
        loginPanel.SetActive(false);
        loadingPanel.SetActive(true);

        string email = emailInput.text;
        string password = passwordInput.text;

        StartCoroutine(LoginRequest(email, password));
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        string url = "http://localhost:3000/login";

        string jsonData = JsonUtility.ToJson(new AuthData(email, password));

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            loadingPanel.SetActive(false);

            if (response.IsSuccess)
            {
                Debug.Log("Login success: " + response.Text);

                PlayerPrefs.SetString("UserEmail", email);

                PlayerPrefs.DeleteKey(ReactionTimeManager.LAST_REACTION_TIME_KEY);
                PlayerPrefs.DeleteKey(ReactionTimeManager.HAS_REACTION_TIME_DATA_KEY);
                PlayerPrefs.Save();

                loginPanel.SetActive(false);

                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                loginPanel.SetActive(true);
                errorText.text = "Login failed: " + response.Text;
            }
        }));
    }

    private void OnRegisterLinkClicked()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    [System.Serializable]
    public class AuthData
    {
        public string email;
        public string password;

        public AuthData(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
}
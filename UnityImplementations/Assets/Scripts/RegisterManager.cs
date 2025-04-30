using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TextMeshProUGUI registerErrorText;
    public Button registerButton;
    public Button loginLink;
    public GameObject loadingPanel;
    public GameObject registerPanel;
    public GameObject loginPanel;
    public string mainMenuSceneName = "Main menu";
    public bool autoLoginAfterRegister = true;

    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        loginLink.onClick.AddListener(OnLoginLinkClicked);
    }

    private void OnRegisterButtonClicked()
    {
        registerPanel.SetActive(false);
        loadingPanel.SetActive(true);

        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;

        StartCoroutine(RegisterRequest(email, password));
    }

    private IEnumerator RegisterRequest(string email, string password)
    {
        string url = "http://localhost:3000/register";

        string jsonData = JsonUtility.ToJson(new AuthData(email, password));

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            if (response.IsSuccess)
            {
                if (autoLoginAfterRegister)
                {
                    StartCoroutine(LoginAfterRegister(email, password));
                }
                else
                {
                    loadingPanel.SetActive(false);
                    registerPanel.SetActive(false);
                    loginPanel.SetActive(true);
                }
            }
            else
            {
                loadingPanel.SetActive(false);
                registerPanel.SetActive(true);
                registerErrorText.text = "Registration failed: " + response.Text;
            }
        }));
    }

    private IEnumerator LoginAfterRegister(string email, string password)
    {
        string url = "http://localhost:3000/login";

        string jsonData = JsonUtility.ToJson(new AuthData(email, password));

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            loadingPanel.SetActive(false);

            if (response.IsSuccess)
            {
                PlayerPrefs.SetString("UserEmail", email);
                PlayerPrefs.Save();

                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                registerPanel.SetActive(false);
                loginPanel.SetActive(true);
            }
        }));
    }

    private void OnLoginLinkClicked()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
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
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (autoLoginAfterRegister)
            {
                // If auto login is enabled, log the user in automatically
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
            registerErrorText.text = "Registration failed: " + request.downloadHandler.text;
        }
    }

    private IEnumerator LoginAfterRegister(string email, string password)
    {
        string url = "http://localhost:3000/login";

        string jsonData = JsonUtility.ToJson(new AuthData(email, password));
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        loadingPanel.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Store the user's email in PlayerPrefs
            PlayerPrefs.SetString("UserEmail", email);
            PlayerPrefs.Save();

            // Load the main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            // If auto login fails, show the login panel
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
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

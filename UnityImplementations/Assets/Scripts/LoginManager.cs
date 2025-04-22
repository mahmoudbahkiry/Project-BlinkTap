using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;

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
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        loadingPanel.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            // You can store the token here if you return it
            Debug.Log("Login success: " + request.downloadHandler.text);

            // Store the user's email in PlayerPrefs for future use
            PlayerPrefs.SetString("UserEmail", email);
            PlayerPrefs.Save();

            loginPanel.SetActive(false);

            // Load the main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            errorText.text = "Login failed: " + request.downloadHandler.text;
        }
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
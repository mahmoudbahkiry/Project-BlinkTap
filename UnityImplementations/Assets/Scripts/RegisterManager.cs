using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

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

        loadingPanel.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
        else
        {
            registerErrorText.text = "Registration failed: " + request.downloadHandler.text;
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

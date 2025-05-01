using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestServerConnection : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string backendUrl = "http://localhost:3000";
    [SerializeField] private string testEmail = "unity-test@example.com";
    [SerializeField] private string testProfession = "Esports Athletes";

    [Header("UI")]
    [SerializeField] private Button testConnectionButton;
    [SerializeField] private Button testGetProfileButton;
    [SerializeField] private Button testSaveProfileButton;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private ScrollRect scrollRect;

    void Start()
    {
        if (testConnectionButton != null)
            testConnectionButton.onClick.AddListener(TestConnectionClick);

        if (testGetProfileButton != null)
            testGetProfileButton.onClick.AddListener(TestGetProfile);

        if (testSaveProfileButton != null)
            testSaveProfileButton.onClick.AddListener(TestSaveProfile);

        if (resultText != null)
            resultText.text = "Test results will appear here";
    }
    public void TestConnectionClick()
    {
        LogMessage("Testing connection to server...");
        StartCoroutine(TestConnection());
    }
    public void TestGetProfile()
    {
        LogMessage($"Testing get profile for {testEmail}...");
        StartCoroutine(TestGetProfileData());
    }
    public void TestSaveProfile()
    {
        LogMessage($"Testing save profile for {testEmail} with profession {testProfession}...");
        StartCoroutine(TestSaveProfileData());
    }

    private IEnumerator TestConnection()
    {
        string url = $"{backendUrl}/debug";
        LogMessage($"Sending GET request to {url}");

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            if (response.IsSuccess)
            {
                string responseText = response.Text;
                LogMessage("Connection successful!");
                LogMessage($"Response: {responseText}");
            }
            else
            {
                LogError($"Connection failed: {response.Error}");
                LogError($"Response code: {response.StatusCode}");
            }
        }));
    }

    private IEnumerator TestGetProfileData()
    {
        string url = $"{backendUrl}/profile?email={RESTClient.EscapeURL(testEmail)}";
        LogMessage($"Sending GET request to {url}");

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            if (response.IsSuccess)
            {
                string responseText = response.Text;
                LogMessage("Get profile successful!");
                LogMessage($"Response: {responseText}");
            }
            else
            {
                LogError($"Get profile failed: {response.Error}");
                LogError($"Response code: {response.StatusCode}");
            }
        }));
    }

    private IEnumerator TestSaveProfileData()
    {
        string url = $"{backendUrl}/debug/echo";

        TestProfileData profileData = new TestProfileData
        {
            email = testEmail,
            profession = testProfession
        };

        string jsonData = JsonUtility.ToJson(profileData);
        LogMessage($"Sending POST to echo endpoint with data: {jsonData}");

        bool shouldContinue = true;
        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            if (response.IsSuccess)
            {
                string echoResponse = response.Text;
                LogMessage("Echo test successful!");
                LogMessage($"Echo response: {echoResponse}");
            }
            else
            {
                LogError($"Echo test failed: {response.Error}");
                LogError($"Response code: {response.StatusCode}");
                LogError("Skipping actual profile save test due to echo failure");
                shouldContinue = false;
            }
        }));

        if (!shouldContinue)
        {
            yield break;
        }

        url = $"{backendUrl}/profile";
        LogMessage($"Sending POST to profile endpoint with data: {jsonData}");

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            if (response.IsSuccess)
            {
                string responseText = response.Text;
                LogMessage("Save profile successful!");
                LogMessage($"Response: {responseText}");
                LogMessage("Verifying saved data...");
                StartCoroutine(VerifySavedData());
            }
            else
            {
                LogError($"Save profile failed: {response.Error}");
                LogError($"Response code: {response.StatusCode}");
                LogError($"Response body: {response.Text}");
                LogError("Request details:");
                LogError($"URL: {url}");
                LogError($"Method: POST");
                LogError($"Headers: Content-Type: application/json");
                LogError($"Body: {jsonData}");
            }
        }));
    }

    private IEnumerator VerifySavedData()
    {
        string url = $"{backendUrl}/profile?email={RESTClient.EscapeURL(testEmail)}";

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            if (response.IsSuccess)
            {
                string responseText = response.Text;
                LogMessage($"Verification response: {responseText}");

                try
                {
                    TestProfileData profileData = JsonUtility.FromJson<TestProfileData>(responseText);
                    if (profileData != null && profileData.profession == testProfession)
                    {
                        LogMessage("Verification SUCCESSFUL! Data matches what was saved.");
                    }
                    else
                    {
                        LogError($"Verification FAILED! Expected profession: {testProfession}, Got: {profileData?.profession ?? "null"}");
                    }
                }
                catch (System.Exception e)
                {
                    LogError($"Error parsing verification data: {e.Message}");
                }
            }
            else
            {
                LogError($"Verification request failed: {response.Error}");
            }
        }));
    }

    private void LogMessage(string message)
    {
        Debug.Log(message);
        if (resultText != null)
        {
            resultText.text += $"\n[{System.DateTime.Now.ToString("HH:mm:ss")}] {message}";
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0;
        }
    }

    private void LogError(string message)
    {
        Debug.LogError(message);
        if (resultText != null)
        {
            resultText.text += $"\n[{System.DateTime.Now.ToString("HH:mm:ss")}] <color=red>ERROR: {message}</color>";
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
[System.Serializable]
public class TestProfileData
{
    public string email;
    public string profession;
}
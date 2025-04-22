using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;

/// <summary>
/// Test script to verify connection to the backend server and test saving profession data
/// </summary>
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
        // Set up buttons
        if (testConnectionButton != null)
            testConnectionButton.onClick.AddListener(TestConnectionClick);

        if (testGetProfileButton != null)
            testGetProfileButton.onClick.AddListener(TestGetProfile);

        if (testSaveProfileButton != null)
            testSaveProfileButton.onClick.AddListener(TestSaveProfile);

        // Initialize UI
        if (resultText != null)
            resultText.text = "Test results will appear here";
    }

    /// <summary>
    /// Test basic connection to the server
    /// </summary>
    public void TestConnectionClick()
    {
        LogMessage("Testing connection to server...");
        StartCoroutine(TestConnection());
    }

    /// <summary>
    /// Test getting profile data from the server
    /// </summary>
    public void TestGetProfile()
    {
        LogMessage($"Testing get profile for {testEmail}...");
        StartCoroutine(TestGetProfileData());
    }

    /// <summary>
    /// Test saving profile data to the server
    /// </summary>
    public void TestSaveProfile()
    {
        LogMessage($"Testing save profile for {testEmail} with profession {testProfession}...");
        StartCoroutine(TestSaveProfileData());
    }

    private IEnumerator TestConnection()
    {
        string url = $"{backendUrl}/debug";
        LogMessage($"Sending GET request to {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                LogMessage("Connection successful!");
                LogMessage($"Response: {response}");
            }
            else
            {
                LogError($"Connection failed: {request.error}");
                LogError($"Response code: {request.responseCode}");
            }
        }
    }

    private IEnumerator TestGetProfileData()
    {
        string url = $"{backendUrl}/profile?email={UnityWebRequest.EscapeURL(testEmail)}";
        LogMessage($"Sending GET request to {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                LogMessage("Get profile successful!");
                LogMessage($"Response: {response}");
            }
            else
            {
                LogError($"Get profile failed: {request.error}");
                LogError($"Response code: {request.responseCode}");
            }
        }
    }

    private IEnumerator TestSaveProfileData()
    {
        string url = $"{backendUrl}/debug/echo"; // First test with the echo endpoint

        // Create the data object
        TestProfileData profileData = new TestProfileData
        {
            email = testEmail,
            profession = testProfession
        };

        // Convert to JSON
        string jsonData = JsonUtility.ToJson(profileData);
        LogMessage($"Sending POST to echo endpoint with data: {jsonData}");

        // Test the echo endpoint first
        using (UnityWebRequest echoRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            echoRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            echoRequest.downloadHandler = new DownloadHandlerBuffer();
            echoRequest.SetRequestHeader("Content-Type", "application/json");

            yield return echoRequest.SendWebRequest();

            if (echoRequest.result == UnityWebRequest.Result.Success)
            {
                string echoResponse = echoRequest.downloadHandler.text;
                LogMessage("Echo test successful!");
                LogMessage($"Echo response: {echoResponse}");
            }
            else
            {
                LogError($"Echo test failed: {echoRequest.error}");
                LogError($"Response code: {echoRequest.responseCode}");
                LogError("Skipping actual profile save test due to echo failure");
                yield break; // Skip the actual test if echo fails
            }
        }

        // Now test the actual profile endpoint
        url = $"{backendUrl}/profile";
        LogMessage($"Sending POST to profile endpoint with data: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                LogMessage("Save profile successful!");
                LogMessage($"Response: {response}");

                // Verify the saved data by retrieving it
                LogMessage("Verifying saved data...");
                StartCoroutine(VerifySavedData());
            }
            else
            {
                LogError($"Save profile failed: {request.error}");
                LogError($"Response code: {request.responseCode}");
                LogError($"Response body: {request.downloadHandler.text}");

                // Log detailed debug info
                LogError("Request details:");
                LogError($"URL: {url}");
                LogError($"Method: POST");
                LogError($"Headers: Content-Type: application/json");
                LogError($"Body: {jsonData}");
            }
        }
    }

    private IEnumerator VerifySavedData()
    {
        string url = $"{backendUrl}/profile?email={UnityWebRequest.EscapeURL(testEmail)}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                LogMessage($"Verification response: {response}");

                try
                {
                    TestProfileData profileData = JsonUtility.FromJson<TestProfileData>(response);
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
                LogError($"Verification request failed: {request.error}");
            }
        }
    }

    private void LogMessage(string message)
    {
        Debug.Log(message);
        if (resultText != null)
        {
            resultText.text += $"\n[{System.DateTime.Now.ToString("HH:mm:ss")}] {message}";
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0; // Scroll to bottom
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
                scrollRect.verticalNormalizedPosition = 0; // Scroll to bottom
        }
    }
}

/// <summary>
/// Data structure for user profile information for testing (separate from ProfileData in ProfilePanelController)
/// </summary>
[System.Serializable]
public class TestProfileData
{
    public string email;
    public string profession;
}
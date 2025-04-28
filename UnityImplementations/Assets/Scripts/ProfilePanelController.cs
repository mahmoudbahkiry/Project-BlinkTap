using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System;

public class ProfilePanelController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TMP_Dropdown professionDropdown;
    [SerializeField] private Button saveButton;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Backend Configuration")]
    [SerializeField] private string backendUrl = "http://localhost:3000";
    [SerializeField] private bool logDetailedNetworkInfo = true;
    [SerializeField] private bool useHttpServiceForTesting = false;

    private MainMenuManager menuManager;
    private string userEmail;
    private bool isSaving = false;

    private void Awake()
    {
        if (titleText == null)
            titleText = transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();

        if (emailText == null)
            emailText = transform.Find("Content/EmailText")?.GetComponent<TextMeshProUGUI>();

        if (professionDropdown == null)
            professionDropdown = transform.Find("Content/ProfessionDropdown")?.GetComponent<TMP_Dropdown>();

        if (saveButton == null)
        {
            saveButton = transform.Find("Buttons/SaveButton")?.GetComponent<Button>();

            if (saveButton == null)
            {
                saveButton = GetComponentInChildren<Button>();

                if (saveButton == null)
                {
                    Button[] allButtons = GetComponentsInChildren<Button>(true);
                    foreach (Button button in allButtons)
                    {
                        if (button.name.ToLower().Contains("save"))
                        {
                            saveButton = button;
                            Debug.Log($"Found save button by name: {button.name}");
                            break;
                        }
                    }
                }
            }
        }

        if (saveButton != null)
        {
            Debug.Log($"Found save button: {saveButton.name}");
        }
        else
        {
            Debug.LogError("Could not find any save button! Please assign it manually in the Inspector.");
        }

        if (backgroundPanel == null)
            backgroundPanel = GetComponent<Image>();

        if (feedbackPanel == null)
        {
            GameObject panel = new GameObject("FeedbackPanel");
            panel.transform.SetParent(transform);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 80);
            rect.localPosition = new Vector3(0, 0, 0);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            GameObject textObj = new GameObject("FeedbackText");
            textObj.transform.SetParent(panel.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            feedbackText = textObj.AddComponent<TextMeshProUGUI>();
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.fontSize = 16;
            feedbackText.color = Color.white;

            feedbackPanel = panel;
            feedbackPanel.SetActive(false);
        }
    }

    void Start()
    {
        menuManager = FindObjectOfType<MainMenuManager>();

        ApplyStyle();

        SetupSaveButton();
    }

    private void SetupSaveButton()
    {
        if (saveButton == null)
        {
            Debug.LogWarning("Save button not assigned! Attempting to find it...");

            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (Button button in allButtons)
            {
                if (button.name.ToLower().Contains("save"))
                {
                    saveButton = button;
                    Debug.Log($"Found save button in SetupSaveButton: {button.name}");
                    break;
                }
            }

            if (saveButton == null && allButtons.Length > 0)
            {
                saveButton = allButtons[0];
                Debug.LogWarning($"Using first available button as save button: {saveButton.name}");
            }

            if (saveButton == null)
            {
                saveButton = CreateSaveButton();
            }
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();

            saveButton.onClick.AddListener(SaveProfileData);

            Debug.Log("Save button listener set up successfully");
        }
        else
        {
            Debug.LogError("Save button reference is missing! Unable to find any buttons to use.");
            Debug.LogError("Please create a Button with 'Save' in its name and place it in the Profile Panel hierarchy.");
        }
    }

    private Button CreateSaveButton()
    {
        Debug.Log("Creating a save button...");

        Transform buttonsContainer = transform.Find("Buttons");
        if (buttonsContainer == null)
        {
            GameObject buttonsObj = new GameObject("Buttons");
            buttonsContainer = buttonsObj.transform;
            buttonsContainer.SetParent(transform);
            RectTransform rect = buttonsObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 50);
            rect.sizeDelta = new Vector2(200, 50);
        }

        GameObject buttonObj = new GameObject("SaveButton");
        buttonObj.transform.SetParent(buttonsContainer);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(160, 40);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0f, 0.8f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "SAVE";
        buttonText.fontSize = 18;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.black;
        buttonText.alignment = TextAlignmentOptions.Center;

        Debug.Log("Save button created successfully");
        return button;
    }

    public void Initialize(string email)
    {
        Debug.Log($"Initializing ProfilePanel with email: {email}");
        userEmail = email;

        if (emailText != null)
            emailText.text = email;

        StartCoroutine(LoadProfileData());

        SetupSaveButton();
    }

    private IEnumerator LoadProfileData()
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("Cannot load profile data: userEmail is null or empty");
            yield break;
        }

        string url = $"{backendUrl}/profile?email={UnityWebRequest.EscapeURL(userEmail)}";
        Debug.Log($"Loading profile data from: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"Profile data received: {response}");

                try
                {
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(response);

                    if (profileData != null && professionDropdown != null)
                    {
                        Debug.Log($"Setting profession dropdown to: {profileData.profession}");

                        for (int i = 0; i < professionDropdown.options.Count; i++)
                        {
                            if (professionDropdown.options[i].text == profileData.profession)
                            {
                                professionDropdown.value = i;
                                Debug.Log($"Set profession dropdown to index {i}");
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Profile data was null or dropdown not found. Data: {response}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing profile data: {e.Message}, Response: {response}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load profile data: {request.error}, Response code: {request.responseCode}");
            }
        }
    }

    public void SaveProfileData()
    {
        Debug.Log("SaveProfileData method called");

        if (isSaving)
        {
            Debug.Log("Save operation already in progress. Please wait.");
            return;
        }

        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("Cannot save profile data: userEmail is null or empty");
            ShowFeedback("Error: User email is missing", false);
            return;
        }

        if (professionDropdown == null)
        {
            Debug.LogError("Cannot save profile data: professionDropdown is null");
            ShowFeedback("Error: Unable to get profession data", false);
            return;
        }

        if (professionDropdown.options.Count <= 0 || professionDropdown.value < 0 || professionDropdown.value >= professionDropdown.options.Count)
        {
            Debug.LogError($"Invalid dropdown selection: value={professionDropdown.value}, options count={professionDropdown.options.Count}");
            ShowFeedback("Error: Invalid profession selection", false);
            return;
        }

        string selectedProfession = professionDropdown.options[professionDropdown.value].text;
        Debug.Log($"Saving profession: {selectedProfession} for user: {userEmail}");

        if (useHttpServiceForTesting)
        {
            StartCoroutine(DirectFirestoreSave(selectedProfession));
            return;
        }

        StartCoroutine(TestConnection(() =>
        {
            StartCoroutine(SendProfileDataToServer(userEmail, selectedProfession));
        }));
    }

    private IEnumerator SendProfileDataToServer(string email, string profession)
    {
        Debug.Log("SendProfileDataToServer method started");

        if (saveButton != null)
        {
            saveButton.interactable = false;
            TextMeshProUGUI buttonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "SAVING...";
            }
        }

        string url = $"{backendUrl}/profile";

        Debug.Log($"Using endpoint: {url}");

        ProfileData profileData = new ProfileData
        {
            email = email,
            profession = profession
        };

        string jsonData = JsonUtility.ToJson(profileData);
        Debug.Log($"Sending profile data: {jsonData} to {url}");

        yield return StartCoroutine(SendWebRequest(url, jsonData, (success, response) =>
        {
            if (saveButton != null)
            {
                saveButton.interactable = true;
                TextMeshProUGUI buttonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "SAVE";
                }
            }

            if (success)
            {
                Debug.Log($"Profile saved successfully. Server response: {response}");

                try
                {
                    ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(response);
                    Debug.Log($"Response details - Success: {serverResponse.success}, Message: {serverResponse.message}");

                    if (serverResponse.success)
                    {
                        ShowFeedback("Profile saved successfully!", Color.green);
                    }
                    else
                    {
                        ShowFeedback($"Error: {serverResponse.message}", Color.red);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing server response: {e.Message}");
                    ShowFeedback("Profile saved, but response format unexpected", Color.yellow);
                }
            }
            else
            {
                Debug.LogError($"Failed to save profile. Error: {response}");

                if (response.Contains("Cannot POST"))
                {
                    Debug.LogWarning("API endpoint issue detected. Falling back to test mode.");
                    StartCoroutine(DirectFirestoreSave(profession));
                }
                else
                {
                    ShowFeedback("Failed to save profile. Please try again.", Color.red);
                }
            }
        }));
    }

    private IEnumerator DirectFirestoreSave(string profession)
    {
        Debug.Log("Using direct Firestore save method for testing");
        ShowFeedback("Server endpoint error - using fallback save method", Color.yellow);

        yield return new WaitForSeconds(1.0f);

        PlayerPrefs.SetString($"Profile_{userEmail}", profession);
        PlayerPrefs.Save();

        Debug.Log($"Saved profile data locally for {userEmail}: {profession}");
        ShowFeedback("Profile data saved locally (Test Mode)", Color.green);
    }

    private IEnumerator TestConnection(System.Action onSuccess = null)
    {
        string url = $"{backendUrl}/debug";
        Debug.Log($"Testing connection to server: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Connection to server successful!");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"Connection to server failed: {request.error}");
                ShowFeedback("Error: Could not connect to server. Make sure the backend server is running.", false);

                if (useHttpServiceForTesting)
                {
                    Debug.Log("Falling back to direct save method");
                    string selectedProfession = professionDropdown.options[professionDropdown.value].text;
                    StartCoroutine(DirectFirestoreSave(selectedProfession));
                }
            }
        }
    }

    private IEnumerator VerifyProfileSaved(string expectedProfession)
    {
        if (logDetailedNetworkInfo)
        {
            Debug.Log("Verifying profile data was saved correctly...");
        }

        yield return new WaitForSeconds(0.5f);

        string url = $"{backendUrl}/profile?email={UnityWebRequest.EscapeURL(userEmail)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;

                if (logDetailedNetworkInfo)
                {
                    Debug.Log($"Verification response: {response}");
                }

                try
                {
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(response);
                    if (profileData != null)
                    {
                        if (profileData.profession == expectedProfession)
                        {
                            if (logDetailedNetworkInfo)
                            {
                                Debug.Log("Verification successful: data matches what was saved");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Verification warning: expected profession '{expectedProfession}' but got '{profileData.profession}'");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error verifying saved data: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Could not verify saved data: {request.error}");
            }
        }
    }

    private void ShowFeedback(string message, bool success)
    {
        if (feedbackPanel != null && feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = success ? Color.green : Color.red;

            feedbackPanel.SetActive(true);

            StartCoroutine(HideFeedbackAfterDelay(3f));
        }
        else
        {
            Debug.Log(success ? $"Success: {message}" : $"Error: {message}");
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackPanel != null && feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;

            feedbackPanel.SetActive(true);

            StartCoroutine(HideFeedbackAfterDelay(3f));
        }
        else
        {
            bool isSuccess = color == Color.green;
            Debug.Log(isSuccess ? $"Success: {message}" : $"Error/Info: {message}");
        }
    }

    private IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }

    void ApplyStyle()
    {
        if (backgroundPanel != null)
            backgroundPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        if (titleText != null)
        {
            titleText.text = "User Profile";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
        }

        Transform emailLabelTransform = transform.Find("Content/EmailLabel");
        if (emailLabelTransform != null)
        {
            TextMeshProUGUI emailLabel = emailLabelTransform.GetComponent<TextMeshProUGUI>();
            if (emailLabel != null)
            {
                emailLabel.text = "Email:";
                emailLabel.fontSize = 18;
                emailLabel.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        Transform professionLabelTransform = transform.Find("Content/ProfessionLabel");
        if (professionLabelTransform != null)
        {
            TextMeshProUGUI professionLabel = professionLabelTransform.GetComponent<TextMeshProUGUI>();
            if (professionLabel != null)
            {
                professionLabel.text = "Profession:";
                professionLabel.fontSize = 18;
                professionLabel.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        if (saveButton != null)
        {
            Image saveButtonImage = saveButton.GetComponent<Image>();
            if (saveButtonImage != null)
                saveButtonImage.color = new Color(0f, 0.8f, 1f);

            TextMeshProUGUI saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (saveButtonText != null)
            {
                saveButtonText.text = "SAVE";
                saveButtonText.color = Color.black;
                saveButtonText.fontStyle = FontStyles.Bold;
            }

            SetupSaveButton();
        }
    }

    private IEnumerator SendWebRequest(string url, string jsonData, Action<bool, string> callback)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        if (logDetailedNetworkInfo)
        {
            Debug.Log($"Sending POST request to: {url}");
            Debug.Log($"Request headers: Content-Type: application/json, Accept: application/json");
            Debug.Log($"Request body: {jsonData}");
        }

        yield return request.SendWebRequest();

        if (logDetailedNetworkInfo)
        {
            Debug.Log($"Request completed. Result: {request.result}, Response Code: {request.responseCode}");
        }

        bool success = request.result == UnityWebRequest.Result.Success;
        string response = success ? request.downloadHandler.text : request.error;

        if (!success && !string.IsNullOrEmpty(request.downloadHandler.text))
        {
            response = request.downloadHandler.text;
            Debug.LogError($"Error response body: {response}");
        }

        callback(success, response);

        request.Dispose();
    }
}

[System.Serializable]
public class ProfileData
{
    public string email;
    public string profession;
}

[System.Serializable]
public class ErrorResponse
{
    public string error;
    public string details;
}

[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string message;
}
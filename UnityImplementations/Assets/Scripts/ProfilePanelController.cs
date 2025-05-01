using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

        string url = $"{backendUrl}/profile?email={RESTClient.EscapeURL(userEmail)}";
        Debug.Log($"Loading profile data from: {url}");

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            if (response.IsSuccess)
            {
                string responseText = response.Text;
                Debug.Log($"Profile data received: {responseText}");

                try
                {
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(responseText);
                    if (profileData != null && !string.IsNullOrEmpty(profileData.profession))
                    {
                        int professionIndex = -1;
                        if (professionDropdown != null)
                        {
                            for (int i = 0; i < professionDropdown.options.Count; i++)
                            {
                                if (professionDropdown.options[i].text == profileData.profession)
                                {
                                    professionIndex = i;
                                    break;
                                }
                            }

                            if (professionIndex >= 0)
                            {
                                professionDropdown.value = professionIndex;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing profile data: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Error loading profile data: {response.Error}");
            }
        }));
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

        StartCoroutine(SendProfileDataToServer(userEmail, selectedProfession));
    }

    private IEnumerator SendProfileDataToServer(string email, string profession)
    {
        yield return StartCoroutine(TestConnection(onSuccess: () =>
        {
            if (useHttpServiceForTesting)
            {
                StartCoroutine(DirectFirestoreSave(profession));
            }
            else
            {
                ProfileData profileData = new ProfileData
                {
                    email = email,
                    profession = profession
                };

                string jsonData = JsonUtility.ToJson(profileData);
                string url = $"{backendUrl}/profile";

                if (logDetailedNetworkInfo)
                {
                    Debug.Log($"Sending profile data to: {url}");
                    Debug.Log($"JSON data: {jsonData}");
                }

                StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
                {
                    if (response.IsSuccess)
                    {
                        Debug.Log("Profile data saved successfully");
                        ShowFeedback("Profile updated successfully!", true);

                        // Verify the data was saved correctly
                        StartCoroutine(VerifyProfileSaved(profession));
                    }
                    else
                    {
                        Debug.LogError($"Error saving profile data: {response.Error}");
                        string errorMsg = "Could not update profile";

                        try
                        {
                            ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(response.Text);
                            if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.error))
                            {
                                errorMsg = errorResponse.error;
                            }
                        }
                        catch { }

                        ShowFeedback(errorMsg, false);
                    }

                    isSaving = false;
                }));
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

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            bool isConnected = response.IsSuccess;

            if (isConnected)
            {
                Debug.Log("Server connection test successful");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"Server connection test failed: {response.Error}");
                ShowFeedback("Could not connect to server", false);
                isSaving = false;
            }
        }));
    }

    private IEnumerator VerifyProfileSaved(string expectedProfession)
    {
        string url = $"{backendUrl}/profile?email={RESTClient.EscapeURL(userEmail)}";

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            if (response.IsSuccess)
            {
                try
                {
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(response.Text);
                    if (profileData != null)
                    {
                        if (profileData.profession != expectedProfession)
                        {
                            Debug.LogWarning($"Verification mismatch: Expected profession '{expectedProfession}' but got '{profileData.profession}'");
                        }
                        else
                        {
                            Debug.Log("Profile data verification successful - data matches what was saved");
                        }
                    }
                    else
                    {
                        Debug.LogError("Verification failed: Could not parse profile data from response");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error during profile verification: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Verification request failed: {response.Error}");
            }
        }));
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
        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            if (response.IsSuccess)
            {
                callback?.Invoke(true, response.Text);
            }
            else
            {
                callback?.Invoke(false, response.Error);
            }
        }));
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
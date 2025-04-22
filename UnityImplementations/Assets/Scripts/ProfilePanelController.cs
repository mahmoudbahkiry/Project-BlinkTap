using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System;

/// <summary>
/// Controls the ProfilePanel UI and functionality
/// </summary>
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
    [SerializeField] private bool useHttpServiceForTesting = false; // Set to true for local testing

    private MainMenuManager menuManager;
    private string userEmail;
    private bool isSaving = false;

    private void Awake()
    {
        // Try to find references if not assigned
        if (titleText == null)
            titleText = transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();

        if (emailText == null)
            emailText = transform.Find("Content/EmailText")?.GetComponent<TextMeshProUGUI>();

        if (professionDropdown == null)
            professionDropdown = transform.Find("Content/ProfessionDropdown")?.GetComponent<TMP_Dropdown>();

        // Try harder to find the save button if it's not assigned
        if (saveButton == null)
        {
            // First try the direct path
            saveButton = transform.Find("Buttons/SaveButton")?.GetComponent<Button>();

            // If that fails, try searching by name
            if (saveButton == null)
            {
                // Search in immediate children
                saveButton = GetComponentInChildren<Button>();

                // If still not found, try to find any button with "save" in its name
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

        // If we found a save button, log it
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

        // Create feedback panel if it doesn't exist
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
        // Find references
        menuManager = FindObjectOfType<MainMenuManager>();

        // Style the panel
        ApplyStyle();

        // Make sure the save button is set up correctly
        SetupSaveButton();
    }

    /// <summary>
    /// Set up the save button with the correct click listener
    /// </summary>
    private void SetupSaveButton()
    {
        // Try to find the save button again if we still don't have it
        if (saveButton == null)
        {
            Debug.LogWarning("Save button not assigned! Attempting to find it...");

            // Search for any button with "save" in the name
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

            // Last resort - just use the first button we find
            if (saveButton == null && allButtons.Length > 0)
            {
                saveButton = allButtons[0];
                Debug.LogWarning($"Using first available button as save button: {saveButton.name}");
            }

            // If still no button, create one
            if (saveButton == null)
            {
                saveButton = CreateSaveButton();
            }
        }

        if (saveButton != null)
        {
            // Clear any existing listeners to prevent duplicates
            saveButton.onClick.RemoveAllListeners();

            // Add our save function
            saveButton.onClick.AddListener(SaveProfileData);

            Debug.Log("Save button listener set up successfully");
        }
        else
        {
            Debug.LogError("Save button reference is missing! Unable to find any buttons to use.");
            Debug.LogError("Please create a Button with 'Save' in its name and place it in the Profile Panel hierarchy.");
        }
    }

    /// <summary>
    /// Creates a save button if one doesn't exist
    /// </summary>
    private Button CreateSaveButton()
    {
        Debug.Log("Creating a save button...");

        // Create a parent container for the button if needed
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

        // Create the button
        GameObject buttonObj = new GameObject("SaveButton");
        buttonObj.transform.SetParent(buttonsContainer);

        // Set up the RectTransform
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(160, 40);

        // Add Image component (button background)
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0f, 0.8f, 1f); // BlinkTap cyan

        // Add Button component
        Button button = buttonObj.AddComponent<Button>();

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);

        // Set up text RectTransform
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Add TextMeshProUGUI component
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "SAVE";
        buttonText.fontSize = 18;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.black;
        buttonText.alignment = TextAlignmentOptions.Center;

        Debug.Log("Save button created successfully");
        return button;
    }

    /// <summary>
    /// Initialize the profile panel with user data
    /// </summary>
    /// <param name="email">The user's email</param>
    public void Initialize(string email)
    {
        Debug.Log($"Initializing ProfilePanel with email: {email}");
        userEmail = email;

        if (emailText != null)
            emailText.text = email;

        // Load any existing profile data
        StartCoroutine(LoadProfileData());

        // Make sure the save button is set up correctly when initializing
        SetupSaveButton();
    }

    /// <summary>
    /// Load the user's profile data from the backend
    /// </summary>
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

                        // Find and set the dropdown value based on profession
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

    /// <summary>
    /// Save the user's profile data to the backend
    /// </summary>
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

        // If we're using direct Firestore for testing, save directly
        if (useHttpServiceForTesting)
        {
            StartCoroutine(DirectFirestoreSave(selectedProfession));
            return;
        }

        // First verify we can connect to the server
        StartCoroutine(TestConnection(() =>
        {
            // Connection successful, proceed with saving profile data
            StartCoroutine(SendProfileDataToServer(userEmail, selectedProfession));
        }));
    }

    /// <summary>
    /// Send the profile data to the backend server
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="profession">The selected profession</param>
    private IEnumerator SendProfileDataToServer(string email, string profession)
    {
        Debug.Log("SendProfileDataToServer method started");

        // Disable save button while sending to prevent multiple clicks
        if (saveButton != null)
        {
            saveButton.interactable = false;
            TextMeshProUGUI buttonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "SAVING...";
            }
        }

        // Use the correct endpoint path - the server logs show it's expecting just /profile, not /api/v1/user/profile
        string url = $"{backendUrl}/profile";

        Debug.Log($"Using endpoint: {url}");

        // Create the JSON data
        ProfileData profileData = new ProfileData
        {
            email = email,
            profession = profession
        };

        string jsonData = JsonUtility.ToJson(profileData);
        Debug.Log($"Sending profile data: {jsonData} to {url}");

        yield return StartCoroutine(SendWebRequest(url, jsonData, (success, response) =>
        {
            // Re-enable save button
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

                // Parse the response and log more details if possible
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

                // Check if the error contains HTML with "Cannot POST" message
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

    /// <summary>
    /// Direct save to Firestore for testing (requires running a server-side script)
    /// </summary>
    private IEnumerator DirectFirestoreSave(string profession)
    {
        Debug.Log("Using direct Firestore save method for testing");
        ShowFeedback("Server endpoint error - using fallback save method", Color.yellow);

        // This is just a simulation - in a real implementation,
        // you would need to configure and use the Firebase SDK for Unity
        yield return new WaitForSeconds(1.0f);

        // Save locally
        PlayerPrefs.SetString($"Profile_{userEmail}", profession);
        PlayerPrefs.Save();

        Debug.Log($"Saved profile data locally for {userEmail}: {profession}");
        ShowFeedback("Profile data saved locally (Test Mode)", Color.green);
    }

    /// <summary>
    /// Test connection to the backend server
    /// </summary>
    private IEnumerator TestConnection(System.Action onSuccess = null)
    {
        string url = $"{backendUrl}/debug";
        Debug.Log($"Testing connection to server: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Set a timeout
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

                // Let's save directly as a fallback (this is just for testing)
                if (useHttpServiceForTesting)
                {
                    Debug.Log("Falling back to direct save method");
                    string selectedProfession = professionDropdown.options[professionDropdown.value].text;
                    StartCoroutine(DirectFirestoreSave(selectedProfession));
                }
            }
        }
    }

    /// <summary>
    /// Verify that the profile data was saved correctly
    /// </summary>
    /// <param name="expectedProfession">The profession that should have been saved</param>
    private IEnumerator VerifyProfileSaved(string expectedProfession)
    {
        if (logDetailedNetworkInfo)
        {
            Debug.Log("Verifying profile data was saved correctly...");
        }

        // Wait a moment to ensure data has been saved
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

            // Hide the feedback panel after 3 seconds
            StartCoroutine(HideFeedbackAfterDelay(3f));
        }
        else
        {
            Debug.Log(success ? $"Success: {message}" : $"Error: {message}");
        }
    }

    // New method that takes a Color parameter and forwards to the boolean version
    private void ShowFeedback(string message, Color color)
    {
        if (feedbackPanel != null && feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;

            feedbackPanel.SetActive(true);

            // Hide the feedback panel after 3 seconds
            StartCoroutine(HideFeedbackAfterDelay(3f));
        }
        else
        {
            // Determine if this is a success based on color (green = success)
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
        // Set panel background
        if (backgroundPanel != null)
            backgroundPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Style title
        if (titleText != null)
        {
            titleText.text = "User Profile";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
        }

        // Style email label
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

        // Style profession label
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

        // Style save button
        if (saveButton != null)
        {
            // Get button image
            Image saveButtonImage = saveButton.GetComponent<Image>();
            if (saveButtonImage != null)
                saveButtonImage.color = new Color(0f, 0.8f, 1f); // BlinkTap cyan

            // Get button text
            TextMeshProUGUI saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (saveButtonText != null)
            {
                saveButtonText.text = "SAVE";
                saveButtonText.color = Color.black;
                saveButtonText.fontStyle = FontStyles.Bold;
            }

            // Make sure the button has an onClick event
            SetupSaveButton();
        }
    }

    /// <summary>
    /// Sends a web request and returns the result through a callback
    /// </summary>
    /// <param name="url">The URL to send the request to</param>
    /// <param name="jsonData">The JSON data to send</param>
    /// <param name="callback">Callback that receives success status and response data</param>
    private IEnumerator SendWebRequest(string url, string jsonData, Action<bool, string> callback)
    {
        // Create a new web request
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // Set up the request data
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        if (logDetailedNetworkInfo)
        {
            Debug.Log($"Sending POST request to: {url}");
            Debug.Log($"Request headers: Content-Type: application/json, Accept: application/json");
            Debug.Log($"Request body: {jsonData}");
        }

        // Send the request
        yield return request.SendWebRequest();

        if (logDetailedNetworkInfo)
        {
            Debug.Log($"Request completed. Result: {request.result}, Response Code: {request.responseCode}");
        }

        bool success = request.result == UnityWebRequest.Result.Success;
        string response = success ? request.downloadHandler.text : request.error;

        if (!success && !string.IsNullOrEmpty(request.downloadHandler.text))
        {
            // If we have an error response text, use that instead of the generic error
            response = request.downloadHandler.text;
            Debug.LogError($"Error response body: {response}");
        }

        // Call the callback with the result
        callback(success, response);

        // Clean up
        request.Dispose();
    }
}

/// <summary>
/// Data structure for user profile information
/// </summary>
[System.Serializable]
public class ProfileData
{
    public string email;
    public string profession;
}

/// <summary>
/// Error response structure for parsing error messages from the server
/// </summary>
[System.Serializable]
public class ErrorResponse
{
    public string error;
    public string details;
}

// Add new class to parse server response
[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string message;
    // Add more fields as needed based on your server response
}
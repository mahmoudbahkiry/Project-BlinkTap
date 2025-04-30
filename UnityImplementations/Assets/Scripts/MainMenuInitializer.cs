using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuManager menuManager;
    [SerializeField] private UIElementStyler styler;

    [Header("UI Containers")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform gameModeContainer;
    [SerializeField] private Transform statsContainer;
    [SerializeField] private Transform navBarContainer;
    [SerializeField] private Transform challengeContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject soloModeButtonPrefab;
    [SerializeField] private GameObject multiplayerModeButtonPrefab;
    [SerializeField] private GameObject statsPanelPrefab;
    [SerializeField] private GameObject navButtonPrefab;
    [SerializeField] private GameObject challengePanelPrefab;

    [Header("Mode Icons")]
    [SerializeField] private Sprite soloModeIcon;
    [SerializeField] private Sprite multiplayerModeIcon;

    [Header("Navigation Icons")]
    [SerializeField] private Sprite homeIcon;
    [SerializeField] private Sprite soloIcon;
    [SerializeField] private Sprite multiplayerIcon;
    [SerializeField] private Sprite profileIcon;

    [Header("App Content")]
    [SerializeField] private string appTitle = "BlinkTap";
    [SerializeField] private TextMeshProUGUI appTitleText;

    [Header("User Data")]
    [SerializeField] private string userName = "John";
    [SerializeField] private int reactionTime = 198;
    [SerializeField] private int reactionTimeChange = 12;

    [Header("UI Labels")]
    [SerializeField] private TextMeshProUGUI welcomeTextDisplay;
    [SerializeField] private TextMeshProUGUI statsLabel;
    [SerializeField] private TextMeshProUGUI gameModesLabel;

    [Header("Profile Panel")]
    [SerializeField] private GameObject profilePanelPrefab;
    [SerializeField] private Transform canvasTransform;

    private Button soloModeButton;
    private Button multiplayerModeButton;

    void Start()
    {
        InitializeFirebaseManager();

        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        if (canvasTransform == null)
            canvasTransform = FindObjectOfType<Canvas>().transform;

        if (appTitleText != null)
            appTitleText.text = appTitle;

        if (welcomeTextDisplay != null)
            welcomeTextDisplay.text = "Welcome back, " + userName;

        if (statsLabel != null)
            statsLabel.text = "Your Stats";

        if (gameModesLabel != null)
            gameModesLabel.text = "Game Modes";

        SetupGameModes();
        SetupStats();
        SetupNavBar();
        SetupChallengePanel();
        SetupProfilePanel();

        if (menuManager != null)
        {
            GameObject multiplayerButtonObj = GameObject.Find("MultiplayerButton");
            if (multiplayerButtonObj != null)
            {
                ModeButtonController controller = multiplayerButtonObj.GetComponent<ModeButtonController>();
                if (controller != null)
                {
                    controller.Configure(ModeType.Multiplayer, "Multiplayer", "Compete with others", null);

                    multiplayerButtonObj.SetActive(true);

                    multiplayerModeButton = multiplayerButtonObj.GetComponent<Button>();

                    if (menuManager != null && multiplayerModeButton != null)
                    {
                        menuManager.SetMultiplayerButton(multiplayerModeButton);
                    }
                }
            }
        }
    }

    private void InitializeFirebaseManager()
    {
        if (PlayerPrefs.HasKey("UserEmail"))
        {
            string userEmail = PlayerPrefs.GetString("UserEmail");

            FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();
            if (firebaseManager == null)
            {
                GameObject firebaseManagerObj = new GameObject("FirebaseManager");
                firebaseManager = firebaseManagerObj.AddComponent<FirebaseManager>();
                DontDestroyOnLoad(firebaseManagerObj);
            }

            firebaseManager.SetUserEmail(userEmail);
            Debug.Log($"MainMenuInitializer: FirebaseManager initialized with user email: {userEmail}");

            if (welcomeTextDisplay != null)
            {
                string username = userEmail.Split('@')[0];
                welcomeTextDisplay.text = "Welcome back, " + username;
            }
        }
        else
        {
            Debug.LogWarning("MainMenuInitializer: No user email found in PlayerPrefs");
        }
    }

    void SetupGameModes()
    {
        if (gameModeContainer == null || soloModeButtonPrefab == null || multiplayerModeButtonPrefab == null)
            return;

        // Check if buttons already exist in the scene
        GameObject soloButtonObj = null;
        GameObject multiplayerButtonObj = null;

        // Look for existing buttons
        foreach (Transform child in gameModeContainer)
        {
            ModeButtonController controller = child.GetComponent<ModeButtonController>();
            if (controller != null)
            {
                if (controller.modeType == ModeType.Solo)
                    soloButtonObj = child.gameObject;
                else if (controller.modeType == ModeType.Multiplayer)
                    multiplayerButtonObj = child.gameObject;
            }
        }

        // Only create buttons if they don't already exist
        if (soloButtonObj == null)
        {
            soloButtonObj = Instantiate(soloModeButtonPrefab, gameModeContainer);
            ConfigureModeButton(
                soloButtonObj,
                ModeType.Solo,
                "Solo",
                "Train your reflexes",
                soloModeIcon);
        }
        else
        {
            // Just ensure the button has the right event handlers
            Button button = soloButtonObj.GetComponent<Button>();
            if (button != null && menuManager != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => menuManager.LoadGameMode("Solo"));
            }
        }

        if (multiplayerButtonObj == null)
        {
            multiplayerButtonObj = Instantiate(multiplayerModeButtonPrefab, gameModeContainer);
            ConfigureModeButton(
                multiplayerButtonObj,
                ModeType.Multiplayer,
                "Multiplayer",
                "Battle friends",
                multiplayerModeIcon);
        }
        else
        {
            // Just ensure the button has the right event handlers
            Button button = multiplayerButtonObj.GetComponent<Button>();
            if (button != null && menuManager != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => menuManager.LoadGameMode("Multiplayer"));

                if (menuManager != null)
                {
                    menuManager.SetMultiplayerButton(button);
                }
            }
        }

        // Ensure multiplayer button is properly registered with menu manager
        if (menuManager != null && multiplayerButtonObj != null)
        {
            Button mpButton = multiplayerButtonObj.GetComponent<Button>();
            if (mpButton != null)
            {
                menuManager.SetMultiplayerButton(mpButton);
            }
        }
    }

    void SetupStats()
    {
        if (statsContainer == null)
            return;

        Transform existingPanelTransform = statsContainer.Find("ReactionTimePanel");
        if (existingPanelTransform == null)
        {
            Debug.LogError("MainMenuInitializer: Could not find ReactionTimePanel in the scene. Please add it manually in the editor.");
            return;
        }

        StatsPanelController statsPanelController = existingPanelTransform.GetComponent<StatsPanelController>();
        if (statsPanelController == null)
        {
            Debug.LogError("MainMenuInitializer: ReactionTimePanel found but missing StatsPanelController component.");
            return;
        }

        Debug.Log("MainMenuInitializer: Using existing ReactionTimePanel");

        if (ReactionTimeManager.HasReactionTimeData())
        {
            float lastAvgTime = ReactionTimeManager.GetLastAverageReactionTime();
            int roundedAvgTime = Mathf.RoundToInt(lastAvgTime);

            statsPanelController.UpdateValue(
                roundedAvgTime + "ms",
                "↑ " + reactionTimeChange + "ms");

            statsPanelController.UpdateValueTextDirectly(roundedAvgTime + "ms");

            Debug.Log($"MainMenuInitializer: Displaying cached reaction time: {roundedAvgTime}ms");
        }
        else
        {

            statsPanelController.UpdateValue(
                "No tests taken yet",
                "");

            statsPanelController.UpdateValueTextDirectly("No tests taken yet");

            Debug.Log("MainMenuInitializer: No cached reaction time data available");
        }

        FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();
        if (firebaseManager == null)
        {
            GameObject firebaseManagerObj = new GameObject("FirebaseManager");
            firebaseManager = firebaseManagerObj.AddComponent<FirebaseManager>();
            DontDestroyOnLoad(firebaseManagerObj);

            if (PlayerPrefs.HasKey("UserEmail"))
            {
                string email = PlayerPrefs.GetString("UserEmail");
                firebaseManager.SetUserEmail(email);
                Debug.Log($"MainMenuInitializer: Set FirebaseManager email to: {email}");
            }
            else
            {
                Debug.LogError("MainMenuInitializer: No UserEmail found in PlayerPrefs");
            }
        }

        firebaseManager.CheckServerConnection((isConnected) =>
        {
            if (isConnected)
            {
                Debug.Log("MainMenuInitializer: Server connection successful, fetching most recent reaction time");

                firebaseManager.GetMostRecentReactionTime((reactionTime) =>
                {
                    if (reactionTime > 0)
                    {
                        int roundedAvgTime = Mathf.RoundToInt(reactionTime);

                        statsPanelController.UpdateValue(
                            roundedAvgTime + "ms",
                            "↑ " + reactionTimeChange + "ms");

                        statsPanelController.UpdateValueTextDirectly(roundedAvgTime + "ms");

                        Debug.Log($"MainMenuInitializer: Updated reaction time from Firebase: {roundedAvgTime}ms");

                        if (menuManager != null)
                        {
                            menuManager.UpdateStats(roundedAvgTime);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("MainMenuInitializer: Server returned 0 or negative reaction time");

                        if (!ReactionTimeManager.HasReactionTimeData())
                        {
                            statsPanelController.UpdateValue("No tests taken yet", "");

                            statsPanelController.UpdateValueTextDirectly("No tests taken yet");

                            Debug.Log("MainMenuInitializer: Displaying 'No tests taken yet'");
                        }
                    }
                });
            }
            else
            {
                Debug.LogError("MainMenuInitializer: Could not connect to server, using cached data only");
            }
        });
    }

    void SetupNavBar()
    {
        if (navBarContainer == null || navButtonPrefab == null)
            return;

        // Check if buttons already exist in the scene
        Transform homeButtonTransform = null;
        Transform profileButtonTransform = null;

        // Look for existing buttons
        foreach (Transform child in navBarContainer)
        {
            TextMeshProUGUI labelText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
            {
                if (labelText.text == "Home")
                    homeButtonTransform = child;
                else if (labelText.text == "Profile")
                    profileButtonTransform = child;
            }
        }

        // Only create buttons if they don't already exist
        if (homeButtonTransform == null)
        {
            CreateNavButton("Home", homeIcon, true);
        }
        else
        {
            // Set up the existing Home button
            Button button = homeButtonTransform.GetComponent<Button>();
            if (button != null && menuManager != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => menuManager.SwitchTab("Home"));
            }
        }

        if (profileButtonTransform == null)
        {
            CreateNavButton("Profile", profileIcon, false);
        }
        else
        {
            // Set up the existing Profile button
            Button button = profileButtonTransform.GetComponent<Button>();
            if (button != null && menuManager != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => menuManager.SwitchTab("Profile"));
                menuManager.profileButton = button;
            }
        }
    }

    void SetupChallengePanel()
    {
        if (challengeContainer == null || challengePanelPrefab == null)
            return;

        // Check if a challenge panel already exists
        ChallengePanelController existingController = null;
        foreach (Transform child in challengeContainer)
        {
            ChallengePanelController controller = child.GetComponent<ChallengePanelController>();
            if (controller != null)
            {
                existingController = controller;
                break;
            }
        }

        // Only create a new panel if one doesn't already exist
        if (existingController == null)
        {
            GameObject challengePanel = Instantiate(challengePanelPrefab, challengeContainer);
            ChallengePanelController challengeController = challengePanel.GetComponent<ChallengePanelController>();
            if (challengeController != null)
            {
                challengeController.UpdateChallengeData(reactionTime);
            }
        }
        else
        {
            // Just update the existing panel's data
            existingController.UpdateChallengeData(reactionTime);
        }
    }

    void SetupProfilePanel()
    {
        if (profilePanelPrefab == null || canvasTransform == null)
            return;

        // Check if profile panel already exists
        GameObject existingProfilePanel = GameObject.Find("ProfilePanel");
        if (existingProfilePanel == null)
        {
            // Only create a new panel if one doesn't exist
            GameObject profilePanel = Instantiate(profilePanelPrefab, canvasTransform);

            TextMeshProUGUI emailText = profilePanel.transform.Find("Content/EmailText")?.GetComponent<TextMeshProUGUI>();
            TMP_Dropdown professionDropdown = profilePanel.transform.Find("Content/ProfessionDropdown")?.GetComponent<TMP_Dropdown>();
            Button saveButton = profilePanel.transform.Find("Buttons/SaveButton")?.GetComponent<Button>();

            if (menuManager != null)
            {
                menuManager.profilePanel = profilePanel;
                menuManager.emailText = emailText;
                menuManager.professionDropdown = professionDropdown;
                menuManager.saveProfileButton = saveButton;
            }

            profilePanel.SetActive(false);
        }
        else if (menuManager != null)
        {
            // Just ensure the existing panel is properly connected to menu manager
            menuManager.profilePanel = existingProfilePanel;

            // Connect existing UI elements if needed
            if (menuManager.emailText == null)
                menuManager.emailText = existingProfilePanel.transform.Find("Content/EmailText")?.GetComponent<TextMeshProUGUI>();

            if (menuManager.professionDropdown == null)
                menuManager.professionDropdown = existingProfilePanel.transform.Find("Content/ProfessionDropdown")?.GetComponent<TMP_Dropdown>();

            if (menuManager.saveProfileButton == null)
                menuManager.saveProfileButton = existingProfilePanel.transform.Find("Buttons/SaveButton")?.GetComponent<Button>();

            existingProfilePanel.SetActive(false);
        }
    }

    private void ConfigureModeButton(GameObject buttonObj, ModeType modeType, string modeName, string description, Sprite icon)
    {
        ModeButtonController controller = buttonObj.GetComponent<ModeButtonController>();

        if (controller != null)
        {
            controller.Configure(modeType, modeName, description, icon);
        }
    }

    void CreateNavButton(string name, Sprite icon, bool isActive)
    {
        GameObject buttonObj = Instantiate(navButtonPrefab, navBarContainer);
        Button button = buttonObj.GetComponent<Button>();

        Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        TextMeshProUGUI labelText = buttonObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (labelText != null)
        {
            labelText.text = name;
        }

        if (button != null && menuManager != null)
        {
            button.onClick.AddListener(() => menuManager.SwitchTab(name));

            if (name == "Profile" && menuManager != null)
            {
                menuManager.profileButton = button;
            }
        }
    }
}
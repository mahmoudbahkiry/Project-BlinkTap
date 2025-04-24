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
    [SerializeField] private Transform contentPanel; // Non-scrollable content panel
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
    [SerializeField] private Transform canvasTransform; // Reference to the canvas for instantiating the profile panel

    // Game mode buttons
    private Button soloModeButton;
    private Button multiplayerModeButton;

    void Start()
    {
        // Set the user email in FirebaseManager if it exists
        InitializeFirebaseManager();

        // Find references if not assigned
        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        if (canvasTransform == null)
            canvasTransform = FindObjectOfType<Canvas>().transform;

        // Initialize UI elements
        if (appTitleText != null)
            appTitleText.text = appTitle;

        // Set welcome text with the user name
        if (welcomeTextDisplay != null)
            welcomeTextDisplay.text = "Welcome back, " + userName;

        // Set section labels
        if (statsLabel != null)
            statsLabel.text = "Your Stats";

        if (gameModesLabel != null)
            gameModesLabel.text = "Game Modes";

        SetupGameModes();
        SetupStats();
        SetupNavBar();
        SetupChallengePanel();
        SetupProfilePanel();

        // Set game mode button references in menu manager
        if (menuManager != null)
        {
            // Find and configure the multiplayer button in the scene
            GameObject multiplayerButtonObj = GameObject.Find("MultiplayerButton");
            if (multiplayerButtonObj != null)
            {
                ModeButtonController controller = multiplayerButtonObj.GetComponent<ModeButtonController>();
                if (controller != null)
                {
                    // Make sure it's set to Multiplayer mode
                    controller.Configure(ModeType.Multiplayer, "Multiplayer", "Compete with others", null);

                    // Ensure the button is active
                    multiplayerButtonObj.SetActive(true);

                    // Get the button component and store it
                    multiplayerModeButton = multiplayerButtonObj.GetComponent<Button>();

                    // Set it in the menu manager
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
        // Check if the user email is stored in PlayerPrefs
        if (PlayerPrefs.HasKey("UserEmail"))
        {
            string userEmail = PlayerPrefs.GetString("UserEmail");

            // Find or create FirebaseManager
            FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();
            if (firebaseManager == null)
            {
                // Create a new FirebaseManager GameObject
                GameObject firebaseManagerObj = new GameObject("FirebaseManager");
                firebaseManager = firebaseManagerObj.AddComponent<FirebaseManager>();
                DontDestroyOnLoad(firebaseManagerObj);
            }

            // Set the user email
            firebaseManager.SetUserEmail(userEmail);
            Debug.Log($"MainMenuInitializer: FirebaseManager initialized with user email: {userEmail}");

            // Update the welcome message if needed
            if (welcomeTextDisplay != null)
            {
                // Extract username from email (optional)
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

        // Clear existing content
        foreach (Transform child in gameModeContainer)
        {
            Destroy(child.gameObject);
        }

        // Create Solo Mode Button
        GameObject soloButtonObj = Instantiate(soloModeButtonPrefab, gameModeContainer);
        ConfigureModeButton(
            soloButtonObj,
            ModeType.Solo,
            "Solo",
            "Train your reflexes",
            soloModeIcon);

        // Create Multiplayer Mode Button
        GameObject multiplayerButtonObj = Instantiate(multiplayerModeButtonPrefab, gameModeContainer);
        ConfigureModeButton(
            multiplayerButtonObj,
            ModeType.Multiplayer,
            "Multiplayer",
            "Battle friends",
            multiplayerModeIcon);

        // Store reference to multiplayer button in menu manager if available
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
        if (statsContainer == null || statsPanelPrefab == null)
            return;

        // Clear existing content
        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create Reaction Time Stats
        GameObject reactionTimePanel = Instantiate(statsPanelPrefab, statsContainer);
        StatsPanelController reactionTimeController = reactionTimePanel.GetComponent<StatsPanelController>();
        if (reactionTimeController != null)
        {
            reactionTimeController.UpdateValue(
                reactionTime + "ms",
                "↑ " + reactionTimeChange + "ms");
        }
    }

    void SetupNavBar()
    {
        if (navBarContainer == null || navButtonPrefab == null)
            return;

        // Clear existing content
        foreach (Transform child in navBarContainer)
        {
            Destroy(child.gameObject);
        }

        // Create Home Button
        CreateNavButton("Home", homeIcon, true);

        // Create Solo Button
        CreateNavButton("Solo", soloIcon, false);

        // Create Multiplayer Button
        CreateNavButton("Multiplayer", multiplayerIcon, false);

        // Create Profile Button
        CreateNavButton("Profile", profileIcon, false);
    }

    void SetupChallengePanel()
    {
        if (challengeContainer == null || challengePanelPrefab == null)
            return;

        // Clear existing content
        foreach (Transform child in challengeContainer)
        {
            Destroy(child.gameObject);
        }

        // Create Challenge Panel
        GameObject challengePanel = Instantiate(challengePanelPrefab, challengeContainer);
        ChallengePanelController challengeController = challengePanel.GetComponent<ChallengePanelController>();
        if (challengeController != null)
        {
            challengeController.UpdateChallengeData(reactionTime);
        }
    }

    void SetupProfilePanel()
    {
        if (profilePanelPrefab == null || canvasTransform == null)
            return;

        // Create Profile Panel
        GameObject profilePanel = Instantiate(profilePanelPrefab, canvasTransform);

        // Find references to UI elements within the profile panel
        TextMeshProUGUI emailText = profilePanel.transform.Find("Content/EmailText")?.GetComponent<TextMeshProUGUI>();
        TMP_Dropdown professionDropdown = profilePanel.transform.Find("Content/ProfessionDropdown")?.GetComponent<TMP_Dropdown>();
        Button saveButton = profilePanel.transform.Find("Buttons/SaveButton")?.GetComponent<Button>();

        // Assign references to the MainMenuManager
        if (menuManager != null)
        {
            menuManager.profilePanel = profilePanel;
            menuManager.emailText = emailText;
            menuManager.professionDropdown = professionDropdown;
            menuManager.saveProfileButton = saveButton;
        }

        // Initially hide the profile panel
        profilePanel.SetActive(false);
    }

    // Helper method to configure mode buttons
    private void ConfigureModeButton(GameObject buttonObj, ModeType modeType, string modeName, string description, Sprite icon)
    {
        ModeButtonController controller = buttonObj.GetComponent<ModeButtonController>();

        if (controller != null)
        {
            controller.Configure(modeType, modeName, description, icon);
        }
    }

    // Helper method to create navigation buttons
    private void CreateNavButton(string name, Sprite icon, bool isActive)
    {
        GameObject buttonObj = Instantiate(navButtonPrefab, navBarContainer);
        Button button = buttonObj.GetComponent<Button>();

        // Set icon
        Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        // Set text
        TextMeshProUGUI labelText = buttonObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (labelText != null)
        {
            labelText.text = name;
        }

        // Set active state
        if (isActive)
        {
            if (iconImage != null)
                iconImage.color = new Color(0f, 0.8f, 1f); // Accent color

            if (labelText != null)
                labelText.color = new Color(0f, 0.8f, 1f); // Accent color
        }

        // Add click listener
        if (button != null && menuManager != null)
        {
            button.onClick.AddListener(() => menuManager.SwitchTab(name));

            // Store reference to profile button if this is the profile button
            if (name == "Profile" && menuManager != null)
            {
                menuManager.profileButton = button;
            }
        }
    }
}
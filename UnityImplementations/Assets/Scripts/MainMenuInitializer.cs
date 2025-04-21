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
    [SerializeField] private GameObject modeButtonPrefab;
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

    void Start()
    {
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
    }

    void SetupGameModes()
    {
        if (gameModeContainer == null || modeButtonPrefab == null)
            return;

        // Clear existing content
        foreach (Transform child in gameModeContainer)
        {
            Destroy(child.gameObject);
        }

        // Create Solo Mode Button
        CreateModeButton(
            ModeType.Solo,
            "Solo",
            "Train your reflexes",
            soloModeIcon);

        // Create Multiplayer Mode Button
        CreateModeButton(
            ModeType.Multiplayer,
            "Multiplayer",
            "Battle friends",
            multiplayerModeIcon);
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

    // Helper method to create mode buttons
    private void CreateModeButton(ModeType modeType, string modeName, string description, Sprite icon)
    {
        GameObject buttonObj = Instantiate(modeButtonPrefab, gameModeContainer);
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
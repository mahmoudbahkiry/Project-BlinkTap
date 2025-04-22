using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("User Info")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private TextMeshProUGUI readyText;

    [Header("Challenge Section")]
    [SerializeField] private TextMeshProUGUI personalBestText;
    [SerializeField] private Button startButton;

    [Header("Player Stats")]
    [SerializeField] private TextMeshProUGUI reactionTimeText;
    [SerializeField] private TextMeshProUGUI reactionTimeChangeText;

    [Header("Game Modes")]
    [SerializeField] private Button soloButton;
    public Button multiplayerButton;

    [Header("Navigation")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button soloNavButton;
    [SerializeField] private Button multiplayerNavButton;
    public Button profileButton;

    [Header("Profile Panel")]
    public GameObject profilePanel;
    public TextMeshProUGUI emailText;
    public TMP_Dropdown professionDropdown;
    public Button saveProfileButton;
    private ProfilePanelController profilePanelController;

    [Header("Main Content")]
    public GameObject contentPanel; // Reference to the main content panel

    // Current active tab
    private string currentTab = "Home";

    // Player data
    private string playerName = "John";
    private int personalBest = 198;
    private int reactionTimeChange = 12;
    private string playerEmail = "john.doe@example.com";
    private string playerProfession = "Sprinters (Track & Field)";

    // List of professions
    private readonly string[] professions = new string[]
    {
        "Boxers",
        "Mixed Martial Artists (MMA)",
        "Fencers",
        "Sprinters (Track & Field)",
        "Goalkeepers (Football/Soccer, Handball, Hockey)",
        "Table Tennis Players",
        "Tennis Players",
        "Baseball Batters",
        "Cricketers (especially batsmen and wicketkeepers)",
        "Formula 1 Drivers / Race Car Drivers",
        "Basketball Players",
        "Football Players (American)",
        "Hockey Players (Ice & Field)",
        "Esports Athletes",
        "Badminton Players",
        "Volleyball Players",
        "Martial Artists (Karate, Taekwondo, etc.)",
        "Skiers and Snowboarders (especially downhill)",
        "Gymnasts",
        "Rugby Players"
    };

    void Start()
    {
        // Get player email from PlayerPrefs if available
        if (PlayerPrefs.HasKey("UserEmail"))
        {
            playerEmail = PlayerPrefs.GetString("UserEmail");
        }

        // Initialize UI elements
        SetupUserInfo();
        SetupChallengeSection();
        SetupPlayerStats();
        SetupButtons();
        SetupProfilePanel();

        // Initialize tabs
        SwitchTab("Home");

        // Ensure multiplayer button is visible - adding extra redundancy
        if (multiplayerButton != null)
        {
            multiplayerButton.gameObject.SetActive(true);
        }
    }

    void SetupUserInfo()
    {
        if (welcomeText != null)
            welcomeText.text = "Welcome back, " + playerName;

        if (readyText != null)
            readyText.text = "Ready to improve your reaction time?";
    }

    void SetupChallengeSection()
    {
        if (personalBestText != null)
            personalBestText.text = "Beat your personal best: " + personalBest + "ms";

        if (startButton != null)
            startButton.onClick.AddListener(StartChallenge);
    }

    void SetupPlayerStats()
    {
        if (reactionTimeText != null)
            reactionTimeText.text = personalBest + "ms";

        if (reactionTimeChangeText != null)
            reactionTimeChangeText.text = "↑ " + reactionTimeChange + "ms";
    }

    void SetupButtons()
    {
        // Game mode buttons
        if (soloButton != null)
        {
            soloButton.onClick.AddListener(() => LoadGameMode("Solo"));
            // Make sure the solo button is active and properly configured
            soloButton.gameObject.SetActive(true);
        }

        if (multiplayerButton != null)
        {
            multiplayerButton.onClick.AddListener(() => LoadGameMode("Multiplayer"));
            // Double ensure the button is active
            multiplayerButton.gameObject.SetActive(true);

            // Add component if missing
            ModeButtonController controller = multiplayerButton.GetComponent<ModeButtonController>();
            if (controller != null)
            {
                controller.Configure(ModeType.Multiplayer, "Multiplayer", "Compete with others", null);
            }
        }

        // Navigation buttons
        if (homeButton != null)
            homeButton.onClick.AddListener(() => SwitchTab("Home"));

        if (soloNavButton != null)
            soloNavButton.onClick.AddListener(() => SwitchTab("Solo"));

        if (multiplayerNavButton != null)
            multiplayerNavButton.onClick.AddListener(() => SwitchTab("Multiplayer"));

        if (profileButton != null)
            profileButton.onClick.AddListener(() => SwitchTab("Profile"));
    }

    void SetupProfilePanel()
    {
        // Get the ProfilePanelController component
        if (profilePanel != null)
        {
            profilePanelController = profilePanel.GetComponent<ProfilePanelController>();

            if (profilePanelController != null)
            {
                // Initialize the profile panel with the user's email
                if (!string.IsNullOrEmpty(playerEmail))
                {
                    Debug.Log($"Initializing ProfilePanelController with email: {playerEmail}");
                    profilePanelController.Initialize(playerEmail);
                }
                else
                {
                    Debug.LogError("Player email is empty or null! Attempting to use PlayerPrefs.");
                    // Try to get email from PlayerPrefs as fallback
                    if (PlayerPrefs.HasKey("UserEmail"))
                    {
                        playerEmail = PlayerPrefs.GetString("UserEmail");
                        Debug.Log($"Using email from PlayerPrefs: {playerEmail}");
                        profilePanelController.Initialize(playerEmail);
                    }
                    else
                    {
                        Debug.LogError("Could not find user email in PlayerPrefs!");
                    }
                }
            }
            else
            {
                Debug.LogError("ProfilePanelController component not found on the profile panel");

                // Try to add the component if it doesn't exist
                profilePanelController = profilePanel.AddComponent<ProfilePanelController>();
                if (profilePanelController != null)
                {
                    Debug.Log("Added ProfilePanelController component to profile panel");
                    profilePanelController.Initialize(playerEmail);
                }
            }
        }
        else
        {
            Debug.LogError("Profile panel reference is missing!");
        }

        // Leave the rest of this function intact below
        // This UI code can remain, even though the profile functionality
        // is now handled by the ProfilePanelController

        // Set email text
        if (emailText != null)
            emailText.text = playerEmail;

        // Setup profession dropdown
        if (professionDropdown != null)
        {
            professionDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (string profession in professions)
            {
                options.Add(new TMP_Dropdown.OptionData(profession));
            }

            professionDropdown.AddOptions(options);

            // Set current value
            int currentIndex = System.Array.IndexOf(professions, playerProfession);
            if (currentIndex >= 0)
                professionDropdown.value = currentIndex;
        }

        // We no longer need this as the ProfilePanelController handles the save functionality
        // if (saveProfileButton != null)
        //    saveProfileButton.onClick.AddListener(SaveProfileChanges);
    }

    void StartChallenge()
    {
        Debug.Log("Starting today's challenge");
        // Load the game scene or start the challenge
        // SceneManager.LoadScene("GameScene");
    }

    public void LoadGameMode(string mode)
    {
        Debug.Log("Loading game mode: " + mode);
        // Load the appropriate game mode scene
        // SceneManager.LoadScene(mode + "Scene");
    }

    public void SwitchTab(string tab)
    {
        // Update current tab
        currentTab = tab;

        // Update UI elements based on the selected tab
        switch (tab)
        {
            case "Home":
                if (contentPanel != null)
                    contentPanel.SetActive(true);
                if (profilePanel != null)
                    profilePanel.SetActive(false);
                break;
            case "Solo":
                if (contentPanel != null)
                    contentPanel.SetActive(true);
                if (profilePanel != null)
                    profilePanel.SetActive(false);
                break;
            case "Multiplayer":
                if (contentPanel != null)
                    contentPanel.SetActive(true);
                if (profilePanel != null)
                    profilePanel.SetActive(false);
                break;
            case "Profile":
                if (contentPanel != null)
                    contentPanel.SetActive(false);
                if (profilePanel != null)
                {
                    profilePanel.SetActive(true);

                    // Reinitialize the profile panel when switching to it
                    if (profilePanelController != null)
                    {
                        profilePanelController.Initialize(playerEmail);
                    }
                }
                break;
        }

        // Update navigation button appearance
        UpdateNavButtonAppearance();
    }

    void UpdateNavButtonAppearance()
    {
        // Helper function to update button appearance based on active tab
        Color activeColor = new Color(0f, 0.8f, 1f); // BlinkTap cyan
        Color inactiveColor = new Color(0.7f, 0.7f, 0.7f);

        // Update home button
        if (homeButton != null)
        {
            TextMeshProUGUI buttonText = homeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Home" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Home" ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // Update solo button
        if (soloNavButton != null)
        {
            TextMeshProUGUI buttonText = soloNavButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Solo" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Solo" ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // Update multiplayer button
        if (multiplayerNavButton != null)
        {
            TextMeshProUGUI buttonText = multiplayerNavButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Multiplayer" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Multiplayer" ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // Update profile button
        if (profileButton != null)
        {
            TextMeshProUGUI buttonText = profileButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Profile" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Profile" ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }

    // This method is no longer needed as the ProfilePanelController handles this functionality
    void SaveProfileChanges()
    {
        // This functionality is now handled by the ProfilePanelController
        // Method kept for backward compatibility
        Debug.Log("SaveProfileChanges in MainMenuManager is deprecated. Using ProfilePanelController instead.");
    }

    // You can add methods to update player stats in real-time here
    public void UpdateStats(int newReactionTime)
    {
        personalBest = newReactionTime;
        // Update UI
        SetupPlayerStats();
    }

    // Public method to set the multiplayer button reference
    public void SetMultiplayerButton(Button button)
    {
        if (button != null)
        {
            multiplayerButton = button;
            multiplayerButton.onClick.AddListener(() => LoadGameMode("Multiplayer"));
            multiplayerButton.gameObject.SetActive(true);
        }
    }
}
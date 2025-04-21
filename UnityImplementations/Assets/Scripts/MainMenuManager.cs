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
    [SerializeField] private Button multiplayerButton;

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
        // Initialize UI elements
        SetupUserInfo();
        SetupChallengeSection();
        SetupPlayerStats();
        SetupButtons();
        SetupProfilePanel();

        // Initialize tabs
        SwitchTab("Home");
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
            soloButton.onClick.AddListener(() => LoadGameMode("Solo"));

        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(() => LoadGameMode("Multiplayer"));

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

        // Setup save button
        if (saveProfileButton != null)
            saveProfileButton.onClick.AddListener(SaveProfileChanges);
    }

    void StartChallenge()
    {
        Debug.Log("Starting today's challenge");
        // Load the game scene or start the challenge
        // SceneManager.LoadScene("GameScene");
    }

    void LoadGameMode(string mode)
    {
        Debug.Log("Loading game mode: " + mode);
        // Load the appropriate game mode scene
        // SceneManager.LoadScene(mode + "Mode");
    }

    public void SwitchTab(string tab)
    {
        Debug.Log("Switching to tab: " + tab);

        // Update current tab
        currentTab = tab;

        // Show/hide profile panel based on tab
        if (profilePanel != null)
        {
            bool showProfile = (tab == "Profile");
            profilePanel.SetActive(showProfile);

            // If showing the profile panel, update the information
            if (showProfile)
            {
                SetupProfilePanel();
            }
        }

        // Update nav button appearances
        UpdateNavButtonAppearance();
    }

    void UpdateNavButtonAppearance()
    {
        // Helper method to update button appearance
        void SetButtonActive(Button button, bool isActive)
        {
            if (button != null)
            {
                // Find icon and label
                Image iconImage = button.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI labelText = button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

                // Update colors
                Color activeColor = new Color(0f, 0.8f, 1f); // Cyan
                Color inactiveColor = new Color(0.7f, 0.7f, 0.7f); // Gray

                if (iconImage != null)
                    iconImage.color = isActive ? activeColor : inactiveColor;

                if (labelText != null)
                    labelText.color = isActive ? activeColor : inactiveColor;
            }
        }

        // Set active state for all nav buttons
        SetButtonActive(homeButton, currentTab == "Home");
        SetButtonActive(soloNavButton, currentTab == "Solo");
        SetButtonActive(multiplayerNavButton, currentTab == "Multiplayer");
        SetButtonActive(profileButton, currentTab == "Profile");
    }

    void SaveProfileChanges()
    {
        if (professionDropdown != null)
        {
            // Save the selected profession
            int selectedIndex = professionDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < professions.Length)
            {
                playerProfession = professions[selectedIndex];
                Debug.Log("Profession changed to: " + playerProfession);
            }
        }
    }

    // You can add methods to update player stats in real-time here
    public void UpdateStats(int newReactionTime)
    {
        personalBest = newReactionTime;
        // Update UI
        SetupPlayerStats();
    }
}
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
    public GameObject contentPanel;

    private string currentTab = "Home";

    private string playerName = "John";
    private int personalBest = 198;
    private int reactionTimeChange = 12;
    private string playerEmail = "john.doe@example.com";
    private string playerProfession = "Sprinters (Track & Field)";

    private readonly string[] professions = new string[]
    {
        "Boxer",
        "Mixed Martial Artist (MMA)",
        "Fencer",
        "Sprinter (Track & Field)",
        "Goalkeeper (Football/Soccer, Handball, Hockey)",
        "Table Tennis Player",
        "Tennis Player",
        "Baseball Batter",
        "Cricketer",
        "Formula 1 Driver / Race Car Driver",
        "Basketball Player",
        "Football Player (American)",
        "Hockey Player (Ice & Field)",
        "Esports Athlete",
        "Badminton Player",
        "Volleyball Player",
        "Martial Artist",
        "Skier and Snowboarder",
        "Gymnast",
        "Rugby Player"
    };

    void Start()
    {
        if (PlayerPrefs.HasKey("UserEmail"))
        {
            playerEmail = PlayerPrefs.GetString("UserEmail");
        }

        SetupUserInfo();
        SetupChallengeSection();
        SetupPlayerStats();
        SetupButtons();
        SetupProfilePanel();

        SwitchTab("Home");

        if (multiplayerButton != null)
        {
            multiplayerButton.gameObject.SetActive(true);
        }
    }

    void OnEnable()
    {
        Debug.Log("MainMenuManager: OnEnable called - refreshing stats");

        SetupPlayerStats();
        SetupChallengeSection();

        FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();
        if (firebaseManager == null)
        {
            GameObject firebaseManagerObj = new GameObject("FirebaseManager");
            firebaseManager = firebaseManagerObj.AddComponent<FirebaseManager>();
            DontDestroyOnLoad(firebaseManagerObj);

            Debug.Log("MainMenuManager: Created new FirebaseManager instance");

            if (PlayerPrefs.HasKey("UserEmail"))
            {
                string email = PlayerPrefs.GetString("UserEmail");
                firebaseManager.SetUserEmail(email);
                Debug.Log($"MainMenuManager: Set Firebase email to {email}");
            }
            else
            {
                Debug.LogError("MainMenuManager: No UserEmail found in PlayerPrefs!");
                return;
            }
        }

        firebaseManager.CheckServerConnection((isConnected) =>
        {
            if (isConnected)
            {
                Debug.Log("MainMenuManager: Server connection successful, fetching most recent reaction time");

                firebaseManager.GetMostRecentReactionTime((reactionTime) =>
                {
                    if (reactionTime > 0)
                    {
                        Debug.Log($"MainMenuManager: Got reaction time from server: {reactionTime}ms");

                        SetupPlayerStats();
                        SetupChallengeSection();
                    }
                    else
                    {
                        Debug.LogWarning("MainMenuManager: Server returned 0 or negative reaction time");
                    }
                });
            }
            else
            {
                Debug.LogError("MainMenuManager: Could not connect to server, using cached data only");
            }
        });
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
        {
            if (ReactionTimeManager.HasReactionTimeData())
            {
                float lastAvgTime = ReactionTimeManager.GetLastAverageReactionTime();
                int roundedAvgTime = Mathf.RoundToInt(lastAvgTime);
                personalBestText.text = "Beat your personal best: " + roundedAvgTime + "ms";
            }
            else
            {
                personalBestText.text = "Record your first reaction time!";
            }
        }

        if (startButton != null)
            startButton.onClick.AddListener(StartChallenge);
    }

    void SetupPlayerStats()
    {
        if (ReactionTimeManager.HasReactionTimeData())
        {
            float lastAvgTime = ReactionTimeManager.GetLastAverageReactionTime();
            int roundedAvgTime = Mathf.RoundToInt(lastAvgTime);

            if (reactionTimeText != null)
                reactionTimeText.text = roundedAvgTime + "ms";

            if (reactionTimeChangeText != null)
                reactionTimeChangeText.text = "↑ " + reactionTimeChange + "ms";
        }
        else
        {
            if (reactionTimeText != null)
                reactionTimeText.text = "No reaction time yet";

            if (reactionTimeChangeText != null)
                reactionTimeChangeText.text = "";
        }
    }

    void SetupButtons()
    {
        if (soloButton != null)
        {
            soloButton.onClick.AddListener(() => LoadGameMode("Solo"));
            soloButton.gameObject.SetActive(true);
        }

        if (multiplayerButton != null)
        {
            multiplayerButton.onClick.AddListener(() => LoadGameMode("Multiplayer"));
            multiplayerButton.gameObject.SetActive(true);

            ModeButtonController controller = multiplayerButton.GetComponent<ModeButtonController>();
            if (controller != null)
            {
                controller.Configure(ModeType.Multiplayer, "Multiplayer", "Compete with others", null);
            }
        }

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
        if (profilePanel != null)
        {
            profilePanelController = profilePanel.GetComponent<ProfilePanelController>();

            if (profilePanelController != null)
            {
                if (!string.IsNullOrEmpty(playerEmail))
                {
                    Debug.Log($"Initializing ProfilePanelController with email: {playerEmail}");
                    profilePanelController.Initialize(playerEmail);
                }
                else
                {
                    Debug.LogError("Player email is empty or null! Attempting to use PlayerPrefs.");
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

        if (emailText != null)
            emailText.text = playerEmail;

        if (professionDropdown != null)
        {
            professionDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (string profession in professions)
            {
                options.Add(new TMP_Dropdown.OptionData(profession));
            }

            professionDropdown.AddOptions(options);

            int currentIndex = System.Array.IndexOf(professions, playerProfession);
            if (currentIndex >= 0)
                professionDropdown.value = currentIndex;
        }
    }

    void StartChallenge()
    {
        Debug.Log("Starting today's challenge");
    }

    public void LoadGameMode(string mode)
    {
        Debug.Log("Loading game mode: " + mode);

        if (mode == "Solo")
        {
            try
            {
                SceneManager.LoadScene("Solo");
                Debug.Log("Loading Solo scene");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load Solo scene. Make sure it's added to Build Settings! Error: " + e.Message);
                Debug.LogWarning("IMPORTANT: Add the Solo scene to your Build Settings in the Unity Editor (File > Build Settings)");
            }
        }
        else if (mode == "Multiplayer")
        {
            try
            {
                SceneManager.LoadScene("Multiplayer");
                Debug.Log("Loading Multiplayer scene");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load Multiplayer scene. Make sure it's added to Build Settings! Error: " + e.Message);
                Debug.LogWarning("IMPORTANT: Add the Multiplayer scene to your Build Settings in the Unity Editor (File > Build Settings)");
            }
        }
    }

    public void SwitchTab(string tab)
    {
        currentTab = tab;

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

                    if (profilePanelController != null)
                    {
                        profilePanelController.Initialize(playerEmail);
                    }
                }
                break;
        }

        UpdateNavButtonAppearance();
    }

    void UpdateNavButtonAppearance()
    {
        Color activeColor = new Color(0f, 0.8f, 1f);
        Color inactiveColor = new Color(0.7f, 0.7f, 0.7f);

        if (homeButton != null)
        {
            TextMeshProUGUI buttonText = homeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Home" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Home" ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        if (soloNavButton != null)
        {
            TextMeshProUGUI buttonText = soloNavButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Solo" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Solo" ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        if (multiplayerNavButton != null)
        {
            TextMeshProUGUI buttonText = multiplayerNavButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = currentTab == "Multiplayer" ? activeColor : inactiveColor;
                buttonText.fontStyle = currentTab == "Multiplayer" ? FontStyles.Bold : FontStyles.Normal;
            }
        }

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

    public void UpdateStats(int newReactionTime)
    {
        SetupPlayerStats();
        SetupChallengeSection();
    }

    public void SetMultiplayerButton(Button button)
    {
        if (button != null)
        {
            multiplayerButton = button;
            multiplayerButton.onClick.AddListener(() => LoadGameMode("Multiplayer"));
            multiplayerButton.gameObject.SetActive(true);
        }
    }

    public void SetSoloButton(Button button)
    {
        if (button != null)
        {
            soloButton = button;
            soloButton.onClick.AddListener(() => LoadGameMode("Solo"));
            soloButton.gameObject.SetActive(true);
            Debug.Log("Solo button reference set and configured");
        }
    }

    public void ForceFetchFromFirebase()
    {
        Debug.Log("MainMenuManager: ForceFetchFromFirebase called - manually attempting to fetch data");

        FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();
        if (firebaseManager == null)
        {
            Debug.LogError("MainMenuManager: No FirebaseManager found");
            return;
        }

        Debug.Log($"MainMenuManager: Using backend URL: {firebaseManager.GetBackendUrl()}");
        firebaseManager.CheckServerConnection((isConnected) =>
        {
            if (isConnected)
            {
                Debug.Log("MainMenuManager: Connection OK, getting reaction time");

                string email = "unknown";
                if (PlayerPrefs.HasKey("UserEmail"))
                {
                    email = PlayerPrefs.GetString("UserEmail");
                }

                Debug.Log($"MainMenuManager: Using email: {email}");

                firebaseManager.GetMostRecentReactionTime((reactionTime) =>
                {
                    Debug.Log($"MainMenuManager: Got reaction time: {reactionTime}ms");
                    if (reactionTime > 0)
                    {
                        StatsPanelController[] controllers = FindObjectsOfType<StatsPanelController>();
                        Debug.Log($"MainMenuManager: Found {controllers.Length} StatsPanelController instances");

                        foreach (StatsPanelController controller in controllers)
                        {
                            Debug.Log($"MainMenuManager: Updating panel: {controller.gameObject.name}");
                            controller.UpdateValueTextDirectly(Mathf.RoundToInt(reactionTime) + "ms");
                        }
                    }
                    else
                    {
                        Debug.LogError("MainMenuManager: Server returned zero or negative reaction time");
                    }
                });
            }
            else
            {
                Debug.LogError("MainMenuManager: Could not connect to server");
            }
        });
    }
}
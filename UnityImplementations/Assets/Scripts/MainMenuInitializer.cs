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

        foreach (Transform child in gameModeContainer)
        {
            Destroy(child.gameObject);
        }

        GameObject soloButtonObj = Instantiate(soloModeButtonPrefab, gameModeContainer);
        ConfigureModeButton(
            soloButtonObj,
            ModeType.Solo,
            "Solo",
            "Train your reflexes",
            soloModeIcon);

        GameObject multiplayerButtonObj = Instantiate(multiplayerModeButtonPrefab, gameModeContainer);
        ConfigureModeButton(
            multiplayerButtonObj,
            ModeType.Multiplayer,
            "Multiplayer",
            "Battle friends",
            multiplayerModeIcon);

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
                "No reaction time yet",
                "");

            statsPanelController.UpdateValueTextDirectly("No reaction time yet");

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
                            statsPanelController.UpdateValue("No reaction time yet", "");

                            statsPanelController.UpdateValueTextDirectly("No reaction time yet");

                            Debug.Log("MainMenuInitializer: Displaying 'No reaction time yet'");
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

        foreach (Transform child in navBarContainer)
        {
            Destroy(child.gameObject);
        }

        CreateNavButton("Home", homeIcon, true);

        CreateNavButton("Solo", soloIcon, false);

        CreateNavButton("Multiplayer", multiplayerIcon, false);

        CreateNavButton("Profile", profileIcon, false);
    }

    void SetupChallengePanel()
    {
        if (challengeContainer == null || challengePanelPrefab == null)
            return;

        foreach (Transform child in challengeContainer)
        {
            Destroy(child.gameObject);
        }

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

    private void ConfigureModeButton(GameObject buttonObj, ModeType modeType, string modeName, string description, Sprite icon)
    {
        ModeButtonController controller = buttonObj.GetComponent<ModeButtonController>();

        if (controller != null)
        {
            controller.Configure(modeType, modeName, description, icon);
        }
    }

    private void CreateNavButton(string name, Sprite icon, bool isActive)
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

        if (isActive)
        {
            if (iconImage != null)
                iconImage.color = new Color(0f, 0.8f, 1f);

            if (labelText != null)
                labelText.color = new Color(0f, 0.8f, 1f);
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
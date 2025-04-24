using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Player Names Popup")]
    [SerializeField] private GameObject playerNamesPopup;
    [SerializeField] private TMP_InputField player1NameInput;
    [SerializeField] private TMP_InputField player2NameInput;
    [SerializeField] private Button startTestButton;

    [Header("Countdown UI")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float countdownDuration = 3f;

    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player2NameText;
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;

    [Header("Split Screen")]
    [SerializeField] private RectTransform dividerLine;
    [SerializeField] private RectTransform player1Area;
    [SerializeField] private RectTransform player2Area;

    [Header("Circle Spawners")]
    [SerializeField] private MultiplayerCircleSpawner player1CircleSpawner;
    [SerializeField] private MultiplayerCircleSpawner player2CircleSpawner;

    [Header("Timer Manager")]
    [SerializeField] private MultiplayerTimerManager timerManager;

    [Header("Results UI")]
    [SerializeField] private GameObject resultsPopup;
    [SerializeField] private TextMeshProUGUI player1ResultText;
    [SerializeField] private TextMeshProUGUI player2ResultText;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button endTestButton;
    [SerializeField] private Button restartButton;

    // Player names storage
    private string player1Name = "Player 1";
    private string player2Name = "Player 2";

    // Player scores
    private int player1Score = 0;
    private int player2Score = 0;

    // Add fields to track reaction times
    private float player1TotalReactionTime = 0f;
    private int player1TapCount = 0;
    private float player2TotalReactionTime = 0f;
    private int player2TapCount = 0;

    // Public getters for player names
    public string Player1Name => player1Name;
    public string Player2Name => player2Name;

    // Singleton pattern
    public static MultiplayerManager Instance { get; private set; }

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate required references
        if (playerNamesPopup == null)
        {
            Debug.LogError("Player Names Popup not assigned in Inspector!");
        }

        if (player1NameInput == null || player2NameInput == null)
        {
            Debug.LogError("Player name input fields not assigned in Inspector!");
        }

        if (startTestButton == null)
        {
            Debug.LogError("Start Test button not assigned in Inspector!");
        }

        if (timerManager == null)
        {
            timerManager = FindObjectOfType<MultiplayerTimerManager>();
            if (timerManager == null)
            {
                Debug.LogError("MultiplayerTimerManager not found in scene!");
            }
        }

        if (player1CircleSpawner == null || player2CircleSpawner == null)
        {
            Debug.LogError("Player circle spawners not assigned in Inspector!");
        }

        if (countdownText == null)
        {
            Debug.LogError("Countdown Text not assigned in Inspector!");
        }

        if (dividerLine == null)
        {
            Debug.LogError("Divider line not assigned in Inspector!");
        }

        if (player1Area == null || player2Area == null)
        {
            Debug.LogError("Player areas not assigned in Inspector!");
        }
    }

    private void Start()
    {
        // Ensure the CircleSpawners don't spawn circles at start
        if (player1CircleSpawner != null)
        {
            player1CircleSpawner.enabled = true;
            player1CircleSpawner.StopSpawning();
        }

        if (player2CircleSpawner != null)
        {
            player2CircleSpawner.enabled = true;
            player2CircleSpawner.StopSpawning();
        }

        // Hide countdown text at start
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        // Hide results popup at start
        if (resultsPopup != null)
        {
            resultsPopup.SetActive(false);
        }

        // Setup the popup
        SetupPlayerNamesPopup();

        // Setup results popup buttons
        SetupResultsPopupButtons();

        // Show the player names popup
        ShowPlayerNamesPopup();
    }

    private void SetupResultsPopupButtons()
    {
        if (endTestButton != null)
        {
            endTestButton.onClick.AddListener(OnEndTestClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    private void SetupPlayerNamesPopup()
    {
        // Set default values for input fields
        if (player1NameInput != null)
        {
            player1NameInput.text = "Player 1";
        }

        if (player2NameInput != null)
        {
            player2NameInput.text = "Player 2";
        }

        // Add button listener
        if (startTestButton != null)
        {
            startTestButton.onClick.AddListener(OnStartTestClicked);
        }
    }

    private void ShowPlayerNamesPopup()
    {
        if (playerNamesPopup != null)
        {
            playerNamesPopup.SetActive(true);
        }
    }

    private void OnStartTestClicked()
    {
        // Get player names from input fields
        if (player1NameInput != null)
        {
            player1Name = player1NameInput.text.Trim();
            if (string.IsNullOrEmpty(player1Name))
            {
                player1Name = "Player 1";
            }
        }

        if (player2NameInput != null)
        {
            player2Name = player2NameInput.text.Trim();
            if (string.IsNullOrEmpty(player2Name))
            {
                player2Name = "Player 2";
            }
        }

        // Log the names (for debugging)
        Debug.Log($"Starting multiplayer test with Player 1: {player1Name}, Player 2: {player2Name}");

        // Hide the popup
        if (playerNamesPopup != null)
        {
            playerNamesPopup.SetActive(false);
        }

        // Update player name display
        UpdatePlayerUI();

        // Reset scores
        ResetScores();

        // Reset reaction times
        ResetReactionTimes();

        // Start the countdown
        StartCoroutine(StartCountdown());
    }

    private void UpdatePlayerUI()
    {
        if (player1NameText != null)
        {
            player1NameText.text = player1Name;
        }

        if (player2NameText != null)
        {
            player2NameText.text = player2Name;
        }

        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = player1Score.ToString();
        }

        if (player2ScoreText != null)
        {
            player2ScoreText.text = player2Score.ToString();
        }
    }

    private void ResetScores()
    {
        player1Score = 0;
        player2Score = 0;
        UpdateScoreDisplay();
    }

    private void ResetReactionTimes()
    {
        player1TotalReactionTime = 0f;
        player1TapCount = 0;
        player2TotalReactionTime = 0f;
        player2TapCount = 0;
    }

    // This method is called from Player 1's CircleBehavior when a circle is tapped
    public void IncrementPlayer1Score()
    {
        player1Score++;
        UpdateScoreDisplay();
    }

    // This method is called from Player 2's CircleBehavior when a circle is tapped
    public void IncrementPlayer2Score()
    {
        player2Score++;
        UpdateScoreDisplay();
    }

    // Add methods to track player reaction times
    public void AddPlayer1ReactionTime(float reactionTimeMs)
    {
        player1TotalReactionTime += reactionTimeMs;
        player1TapCount++;
    }

    public void AddPlayer2ReactionTime(float reactionTimeMs)
    {
        player2TotalReactionTime += reactionTimeMs;
        player2TapCount++;
    }

    // Get average reaction times (in ms)
    public float GetPlayer1AverageReactionTime()
    {
        if (player1TapCount == 0) return 0f;
        return player1TotalReactionTime / player1TapCount;
    }

    public float GetPlayer2AverageReactionTime()
    {
        if (player2TapCount == 0) return 0f;
        return player2TotalReactionTime / player2TapCount;
    }

    private IEnumerator StartCountdown()
    {
        Debug.Log("MultiplayerManager: Starting countdown sequence");

        // Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // 3, 2, 1 countdown
        for (int i = (int)countdownDuration; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                Debug.Log("MultiplayerManager: Countdown - " + i);
            }
            yield return new WaitForSeconds(1f);
        }

        // Show START! text
        if (countdownText != null)
        {
            countdownText.text = "START!";
            Debug.Log("MultiplayerManager: Countdown - START!");
        }

        yield return new WaitForSeconds(0.5f);

        // Hide countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        // Start the test
        StartTest();
    }

    private void StartTest()
    {
        Debug.Log("MultiplayerManager: StartTest called - Starting the test now");

        // Start the timer manager
        if (timerManager != null)
        {
            timerManager.StartTest();
        }
        else
        {
            Debug.LogError("MultiplayerManager: TimerManager is null, cannot start test!");
        }
    }

    // Call this when the test is completed
    public void OnTestCompleted()
    {
        ShowResultsPopup();
    }

    private void ShowResultsPopup()
    {
        if (resultsPopup != null)
        {
            resultsPopup.SetActive(true);

            float player1AvgTime = GetPlayer1AverageReactionTime();
            float player2AvgTime = GetPlayer2AverageReactionTime();

            // Display player results with reaction times
            if (player1ResultText != null)
            {
                player1ResultText.text = $"{player1Name}: {player1Score} points\nAvg Reaction: {player1AvgTime:F0} ms";
            }

            if (player2ResultText != null)
            {
                player2ResultText.text = $"{player2Name}: {player2Score} points\nAvg Reaction: {player2AvgTime:F0} ms";
            }

            // Determine winner based on reaction time (lower is better)
            if (winnerText != null)
            {
                // Only compare if both players have valid reaction times
                if (player1TapCount > 0 && player2TapCount > 0)
                {
                    if (player1AvgTime < player2AvgTime)
                    {
                        winnerText.text = $"{player1Name} wins with faster reactions!";
                    }
                    else if (player2AvgTime < player1AvgTime)
                    {
                        winnerText.text = $"{player2Name} wins with faster reactions!";
                    }
                    else
                    {
                        winnerText.text = "It's a tie!";
                    }
                }
                else if (player1TapCount > 0)
                {
                    winnerText.text = $"{player1Name} wins by default!";
                }
                else if (player2TapCount > 0)
                {
                    winnerText.text = $"{player2Name} wins by default!";
                }
                else
                {
                    winnerText.text = "No winner - no circles tapped!";
                }
            }
        }
    }

    private void OnEndTestClicked()
    {
        // Return to main menu or perform other end actions
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main menu");
    }

    private void OnRestartClicked()
    {
        // Hide results popup
        if (resultsPopup != null)
        {
            resultsPopup.SetActive(false);
        }

        // Show player names popup again
        ShowPlayerNamesPopup();
    }
}

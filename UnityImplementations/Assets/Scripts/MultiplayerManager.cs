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
    [SerializeField] private GameObject playerScoresPanel;
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

    private string player1Name = "Player 1";
    private string player2Name = "Player 2";

    private int player1Score = 0;
    private int player2Score = 0;

    private float player1TotalReactionTime = 0f;
    private int player1TapCount = 0;
    private float player2TotalReactionTime = 0f;
    private int player2TapCount = 0;

    public string Player1Name => player1Name;
    public string Player2Name => player2Name;

    public static MultiplayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        if (dividerLine != null)
        {
            dividerLine.gameObject.SetActive(false);
        }

        if (playerScoresPanel != null)
        {
            playerScoresPanel.SetActive(false);
        }

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

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (resultsPopup != null)
        {
            resultsPopup.SetActive(false);
        }

        SetupPlayerNamesPopup();

        SetupResultsPopupButtons();

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
        if (player1NameInput != null)
        {
            player1NameInput.text = "Player 1";
        }

        if (player2NameInput != null)
        {
            player2NameInput.text = "Player 2";
        }

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

        Debug.Log($"Starting multiplayer test with Player 1: {player1Name}, Player 2: {player2Name}");

        if (playerNamesPopup != null)
        {
            playerNamesPopup.SetActive(false);
        }

        UpdatePlayerUI();

        ResetScores();

        ResetReactionTimes();

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

    public void IncrementPlayer1Score()
    {
        player1Score++;
        UpdateScoreDisplay();
    }

    public void IncrementPlayer2Score()
    {
        player2Score++;
        UpdateScoreDisplay();
    }

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

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                Debug.Log("MultiplayerManager: Countdown - " + i);
            }
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "START!";
            Debug.Log("MultiplayerManager: Countdown - START!");
        }

        yield return new WaitForSeconds(0.5f);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        StartTest();
    }

    private void StartTest()
    {
        Debug.Log("MultiplayerManager: StartTest called - Starting the test now");

        if (dividerLine != null)
        {
            dividerLine.gameObject.SetActive(true);
        }

        if (playerScoresPanel != null)
        {
            playerScoresPanel.SetActive(true);
        }

        if (timerManager != null)
        {
            timerManager.StartTest();
        }
        else
        {
            Debug.LogError("MultiplayerManager: TimerManager is null, cannot start test!");
        }
    }

    public void OnTestCompleted()
    {
        if (playerScoresPanel != null)
        {
            playerScoresPanel.SetActive(false);
        }

        ShowResultsPopup();
    }

    private void ShowResultsPopup()
    {
        if (resultsPopup != null)
        {
            resultsPopup.SetActive(true);

            float player1AvgTime = GetPlayer1AverageReactionTime();
            float player2AvgTime = GetPlayer2AverageReactionTime();

            if (player1ResultText != null)
            {
                player1ResultText.text = $"{player1Name}: {player1Score} points\nAvg Reaction: {player1AvgTime:F0} ms";
            }

            if (player2ResultText != null)
            {
                player2ResultText.text = $"{player2Name}: {player2Score} points\nAvg Reaction: {player2AvgTime:F0} ms";
            }

            if (winnerText != null)
            {
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main menu");
    }

    private void OnRestartClicked()
    {
        if (dividerLine != null)
        {
            dividerLine.gameObject.SetActive(false);
        }

        if (playerScoresPanel != null)
        {
            playerScoresPanel.SetActive(false);
        }

        if (resultsPopup != null)
        {
            resultsPopup.SetActive(false);
        }

        ShowPlayerNamesPopup();
    }
}

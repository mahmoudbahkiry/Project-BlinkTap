using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengePanelController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startButtonText;

    // References
    private UIElementStyler styler;
    private MainMenuManager menuManager;

    [Header("Challenge Data")]
    [SerializeField] private string challengeTitle = "Today's Challenge";
    [SerializeField] private int personalBest = 198;

    private void Awake()
    {
        // Try to find references if not assigned
        if (backgroundPanel == null)
            backgroundPanel = GetComponent<Image>();

        if (titleText == null)
            titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();

        if (bestScoreText == null)
            bestScoreText = transform.Find("BestScore")?.GetComponent<TextMeshProUGUI>();

        if (startButton == null)
            startButton = transform.Find("StartButton")?.GetComponent<Button>();

        if (startButtonText == null && startButton != null)
            startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        // Find references in scene
        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        // Apply initial settings
        SetupChallengePanel();

        // Add button listener
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    public void SetupChallengePanel()
    {
        // Set text values
        if (titleText != null)
        {
            titleText.text = challengeTitle;
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = "Beat your personal best: " + personalBest + "ms";
        }

        if (startButtonText != null)
        {
            startButtonText.text = "START";
        }

        // Apply styling
        if (styler != null)
        {
            styler.StyleChallengePanel(backgroundPanel, titleText, bestScoreText, null, startButton);
        }
    }

    // Update challenge data
    public void UpdateChallengeData(int newPersonalBest)
    {
        personalBest = newPersonalBest;
        SetupChallengePanel();
    }

    private void OnStartButtonClicked()
    {
        // Forward event to menu manager
        if (menuManager != null)
        {
            Debug.Log("Starting today's challenge");
            // Call method on menu manager - this would likely be handled by an event
        }
    }
}
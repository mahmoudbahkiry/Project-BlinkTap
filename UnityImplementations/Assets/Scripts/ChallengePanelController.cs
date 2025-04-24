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
    private FirebaseManager firebaseManager;

    [Header("Challenge Data")]
    [SerializeField] private string challengeTitle = "Today's Challenge";
    [SerializeField] private int personalBest = 0; // Default to 0 for new users
    private bool isFetchingBestScore = false;

    private void Awake()
    {
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
        Debug.Log($"ChallengePanelController: Starting initialization on GameObject: {gameObject.name}");

        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        if (firebaseManager == null)
        {
            firebaseManager = FirebaseManager.Instance;
            Debug.Log("ChallengePanelController: FirebaseManager instance obtained");
        }

        // Debug the hierarchy to help find the BestScoreText
        Debug.Log($"ChallengePanelController: GameObject path: {GetGameObjectPath(gameObject)}");

        // Double-check the bestScoreText reference
        if (bestScoreText == null)
        {
            Debug.LogWarning("ChallengePanelController: bestScoreText is null, trying to find it again...");

            // Try direct path first
            bestScoreText = transform.Find("BestScore")?.GetComponent<TextMeshProUGUI>();

            // If still null, try a more comprehensive search
            if (bestScoreText == null)
            {
                Debug.LogWarning("ChallengePanelController: Still couldn't find by direct path, trying GetComponentInChildren...");
                // Search in children
                bestScoreText = GetComponentInChildren<TextMeshProUGUI>(true);

                // If we found any TextMeshProUGUI component, log it
                if (bestScoreText != null)
                {
                    Debug.Log($"ChallengePanelController: Found a TextMeshProUGUI component: {bestScoreText.name} with text: {bestScoreText.text}");
                }

                // Search for any TMPro components
                TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
                Debug.Log($"ChallengePanelController: Found {allTexts.Length} TextMeshProUGUI components:");
                foreach (TextMeshProUGUI text in allTexts)
                {
                    Debug.Log($"  - {GetGameObjectPath(text.gameObject)}: '{text.text}'");
                    // If any of them have "best" in their name or text, use it
                    if (text.name.ToLower().Contains("best") || text.text.ToLower().Contains("best"))
                    {
                        bestScoreText = text;
                        Debug.Log($"ChallengePanelController: Found likely best score text: {text.name}");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"ChallengePanelController: bestScoreText is assigned: {bestScoreText.name} with text: {bestScoreText.text}");
        }

        // Show default panel while we fetch the best score
        SetupChallengePanel();

        // Fetch the best score from Firebase
        FetchBestScore();

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void FetchBestScore()
    {
        if (isFetchingBestScore || firebaseManager == null)
            return;

        isFetchingBestScore = true;
        Debug.Log("ChallengePanelController: Starting to fetch best score...");

        firebaseManager.GetBestScore((bestScore) =>
        {
            isFetchingBestScore = false;

            if (bestScore > 0)
            {
                // Only update if we got a valid score
                Debug.Log($"ChallengePanelController: Best score received from Firebase: {bestScore}ms");
                personalBest = bestScore;
                // Force update in the next frame
                StartCoroutine(UpdateTextNextFrame());
            }
            else
            {
                Debug.Log("ChallengePanelController: No best score found or not logged in");
                // Set personalBest to 0 to indicate no best score
                personalBest = 0;
                StartCoroutine(UpdateTextNextFrame());
            }
        });
    }

    private IEnumerator UpdateTextNextFrame()
    {
        yield return null; // Wait for next frame
        Debug.Log($"ChallengePanelController: Updating best score text to: {personalBest}ms");

        if (bestScoreText == null)
        {
            Debug.LogError("ChallengePanelController: bestScoreText is null! Cannot update UI.");
            // Try to find it again
            bestScoreText = transform.Find("BestScore")?.GetComponent<TextMeshProUGUI>();
            if (bestScoreText == null)
            {
                Debug.LogError("ChallengePanelController: Still couldn't find bestScoreText component!");
                yield break;
            }
        }

        UpdateBestScoreText();
        Debug.Log($"ChallengePanelController: Text updated successfully to: {bestScoreText.text}");
    }

    public void SetupChallengePanel()
    {
        if (titleText != null)
        {
            titleText.text = challengeTitle;
        }

        UpdateBestScoreText();

        if (startButtonText != null)
        {
            startButtonText.text = "START";
        }

        if (styler != null)
        {
            styler.StyleChallengePanel(backgroundPanel, titleText, bestScoreText, null, startButton);
        }
    }

    private void UpdateBestScoreText()
    {
        if (bestScoreText != null)
        {
            // Check if this is a new user with no best score
            if (personalBest <= 0)
            {
                // Display message for new users
                bestScoreText.text = "No personal best yet, play now!";

                // Set the font size to make it larger
                bestScoreText.fontSize = 30f;

                // Make it bold for better visibility
                bestScoreText.fontStyle = TMPro.FontStyles.Bold;

                // Optional: Add color to make the call to action stand out
                bestScoreText.text = "No personal best yet, <color=#00CCFF>play now!</color>";
            }
            else
            {
                // For returning users with a best score
                bestScoreText.text = $"Beat your personal best: <color=#00CCFF>{personalBest}</color>ms";

                // Set the font size to make it larger
                bestScoreText.fontSize = 30f;

                // Make it bold for better visibility
                bestScoreText.fontStyle = TMPro.FontStyles.Bold;
            }

            // Common settings for both cases
            bestScoreText.characterSpacing = 1f;
            bestScoreText.alignment = TMPro.TextAlignmentOptions.Center;

            Debug.Log($"ChallengePanelController: Updated best score text: {bestScoreText.text}, Font size: {bestScoreText.fontSize}");
        }
        else
        {
            Debug.LogError("ChallengePanelController: bestScoreText is null in UpdateBestScoreText!");
        }
    }

    public void UpdateChallengeData(int newPersonalBest)
    {
        personalBest = newPersonalBest;
        UpdateBestScoreText();
    }

    private void OnStartButtonClicked()
    {
        if (menuManager != null)
        {
            Debug.Log("Starting today's challenge");
        }
    }

    // Helper method to get the full path of a GameObject
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
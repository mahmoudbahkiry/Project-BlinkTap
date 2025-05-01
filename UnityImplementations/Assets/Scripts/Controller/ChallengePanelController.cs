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

    private Color initialBackgroundColor;
    private Vector2 initialAnchoredPosition;
    private Vector2 initialSizeDelta;
    private Vector2 initialAnchorMin;
    private Vector2 initialAnchorMax;

    private UIElementStyler styler;
    private MainMenuManager menuManager;
    private FirebaseManager firebaseManager;

    [Header("Challenge Data")]
    [SerializeField] private string challengeTitle = "Today's Challenge";
    [SerializeField] private int personalBest = 0;
    private bool isFetchingBestScore = false;

    private void Awake()
    {
        RectTransform rt = transform as RectTransform;
        if (rt != null)
        {
            initialAnchoredPosition = rt.anchoredPosition;
            initialSizeDelta = rt.sizeDelta;
            initialAnchorMin = rt.anchorMin;
            initialAnchorMax = rt.anchorMax;
        }

        if (backgroundPanel == null)
            backgroundPanel = GetComponent<Image>();

        if (backgroundPanel != null)
            initialBackgroundColor = backgroundPanel.color;

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

        Debug.Log($"ChallengePanelController: GameObject path: {GetGameObjectPath(gameObject)}");

        if (bestScoreText == null)
        {
            Debug.LogWarning("ChallengePanelController: bestScoreText is null, trying to find it again...");

            bestScoreText = transform.Find("BestScore")?.GetComponent<TextMeshProUGUI>();

            if (bestScoreText == null)
            {
                Debug.LogWarning("ChallengePanelController: Still couldn't find by direct path, trying GetComponentInChildren...");

                bestScoreText = GetComponentInChildren<TextMeshProUGUI>(true);

                if (bestScoreText != null)
                {
                    Debug.Log($"ChallengePanelController: Found a TextMeshProUGUI component: {bestScoreText.name} with text: {bestScoreText.text}");
                }

                TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
                Debug.Log($"ChallengePanelController: Found {allTexts.Length} TextMeshProUGUI components:");
                foreach (TextMeshProUGUI text in allTexts)
                {
                    Debug.Log($"  - {GetGameObjectPath(text.gameObject)}: '{text.text}'");
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

        SetupTexts();

        FetchBestScore();

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void Update()
    {
        MaintainOriginalAppearance();
    }

    private void MaintainOriginalAppearance()
    {
        RectTransform rt = transform as RectTransform;
        if (rt != null)
        {
            rt.anchoredPosition = initialAnchoredPosition;
            rt.sizeDelta = initialSizeDelta;
            rt.anchorMin = initialAnchorMin;
            rt.anchorMax = initialAnchorMax;
        }

        if (backgroundPanel != null)
        {
            backgroundPanel.color = initialBackgroundColor;
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
                Debug.Log($"ChallengePanelController: Best score received from Firebase: {bestScore}ms");
                personalBest = bestScore;
                StartCoroutine(UpdateTextNextFrame());
            }
            else
            {
                Debug.Log("ChallengePanelController: No best score found or not logged in");
                personalBest = 0;
                StartCoroutine(UpdateTextNextFrame());
            }
        });
    }

    private IEnumerator UpdateTextNextFrame()
    {
        yield return null;
        Debug.Log($"ChallengePanelController: Updating best score text to: {personalBest}ms");

        if (bestScoreText == null)
        {
            Debug.LogError("ChallengePanelController: bestScoreText is null! Cannot update UI.");
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

    public void SetupTexts()
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
    }

    private void UpdateBestScoreText()
    {
        if (bestScoreText != null)
        {
            if (personalBest <= 0)
            {
                bestScoreText.text = "No personal best yet, <color=#00CCFF>play now!</color>";
            }
            else
            {
                bestScoreText.text = $"Beat your personal best: <color=#00CCFF>{personalBest}</color>ms";
            }
            Debug.Log($"ChallengePanelController: Updated best score text: {bestScoreText.text}");
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
            menuManager.LoadGameMode("Solo");
        }
        else
        {
            Debug.LogWarning("ChallengePanelController: menuManager is null, trying to load Solo scene directly");
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Solo");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load Solo scene: " + e.Message);
            }
        }
    }

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
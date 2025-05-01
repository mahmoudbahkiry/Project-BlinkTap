using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TestTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float testDuration = 30f;
    [SerializeField] private float countdownDuration = 3f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject circleSpawnerObject;
    [SerializeField] private GameObject mainContentObject;

    [Header("Results Popup")]
    [SerializeField] private GameObject resultsPopup;
    [SerializeField] private TextMeshProUGUI averageReactionTimeText;
    [SerializeField] private Button endTestButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI uploadStatusText;

    private bool testInProgress = false;
    private float testTimer;
    private CircleSpawner circleSpawner;
    private float lastAverageReactionTime = 0f;

    public bool IsTestActive()
    {
        return testInProgress;
    }

    private void Awake()
    {
        AddTagIfNotExists("Circle");

        if (circleSpawnerObject != null)
        {
            circleSpawner = circleSpawnerObject.GetComponent<CircleSpawner>();
            if (circleSpawner == null)
            {
                Debug.LogError("CircleSpawner component not found on the specified GameObject!");
            }
        }
        else
        {
            Debug.LogError("CircleSpawnerObject not assigned in Inspector!");
        }

        if (mainContentObject == null)
        {
            Debug.LogError("MainContent object not assigned in Inspector!");
        }

        if (uploadStatusText != null)
        {
            uploadStatusText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (mainContentObject != null)
        {
            mainContentObject.SetActive(true);
        }

        if (circleSpawner != null)
        {
            circleSpawner.enabled = true;
            circleSpawner.StopSpawning();
            Debug.Log("TestTimerManager: Initialized CircleSpawner (not spawning yet)");
        }

        if (resultsPopup != null)
        {
            resultsPopup.SetActive(false);
        }
        else
        {
            Debug.LogError("Results popup not assigned in Inspector!");
        }

        if (timerText != null)
        {
            timerText.text = $"Time: {testDuration:00.0}";
        }

        if (endTestButton != null)
        {
            endTestButton.onClick.AddListener(OnEndTestClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        Debug.Log("TestTimerManager: Starting countdown sequence");

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                Debug.Log("TestTimerManager: Countdown - " + i);
            }
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "START!";
            Debug.Log("TestTimerManager: Countdown - START!");
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
        Debug.Log("TestTimerManager: StartTest called - Starting the test now");

        testInProgress = true;
        testTimer = testDuration;

        if (ReactionTimeManager.Instance != null)
        {
            ReactionTimeManager.Instance.ResetReactionTimes();
        }
        else
        {
            Debug.LogError("ReactionTimeManager instance not found!");
        }

        if (circleSpawner != null)
        {
            circleSpawner.StopSpawning();

            circleSpawner.enabled = true;
            circleSpawner.StartSpawning();
            Debug.Log("TestTimerManager: Circle spawner explicitly activated to START spawning");
        }
        else
        {
            Debug.LogError("CircleSpawner not found - cannot start test properly!");
        }
    }

    private void Update()
    {
        if (testInProgress)
        {
            testTimer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = $"Time: {testTimer:00.0}";
            }

            if (testTimer <= 0)
            {
                Debug.Log("TestTimerManager: Timer reached zero - ending test");
                EndTest();
            }
        }
    }

    private void EndTest()
    {
        Debug.Log("TestTimerManager: EndTest called - Test is now over");
        testInProgress = false;

        if (timerText != null)
        {
            timerText.text = "Time: 00.0";
        }

        if (circleSpawner != null)
        {
            circleSpawner.StopSpawning();

            circleSpawner.enabled = false;
            Debug.Log("TestTimerManager: Circle spawner explicitly STOPPED and disabled");
        }
        else
        {
            Debug.LogError("CircleSpawner not found - cannot properly stop it!");
        }

        try
        {
            if (HasTag("Circle"))
            {
                GameObject[] remainingCircles = GameObject.FindGameObjectsWithTag("Circle");
                foreach (GameObject circle in remainingCircles)
                {
                    Destroy(circle);
                }
                Debug.Log($"TestTimerManager: Destroyed {remainingCircles.Length} remaining circles");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not destroy circles: " + e.Message);
        }

        if (countdownText != null && countdownText.gameObject.activeSelf)
        {
            countdownText.gameObject.SetActive(false);
        }

        ShowResultsPopup();
    }

    private void ShowResultsPopup()
    {
        if (mainContentObject != null)
        {
            mainContentObject.SetActive(false);
            Debug.Log("TestTimerManager: MainContent hidden");
        }

        if (resultsPopup != null)
        {
            resultsPopup.SetActive(true);
            Debug.Log("TestTimerManager: Results popup displayed");

            if (averageReactionTimeText != null && ReactionTimeManager.Instance != null)
            {
                lastAverageReactionTime = ReactionTimeManager.Instance.AverageReactionTime;
                averageReactionTimeText.text = $"Average Reaction Time: {lastAverageReactionTime:F0} ms";
                Debug.Log($"TestTimerManager: Average reaction time: {lastAverageReactionTime:F0} ms");

                UploadScoreToFirebase(lastAverageReactionTime);
            }
        }
        else
        {
            Debug.LogError("Results popup is not assigned! Please assign it in the inspector.");
        }
    }

    private void UploadScoreToFirebase(float averageReactionTime)
    {
        FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();

        if (firebaseManager == null)
        {
            Debug.Log("TestTimerManager: FirebaseManager not found, creating one...");
            GameObject firebaseManagerObj = new GameObject("FirebaseManager");
            firebaseManager = firebaseManagerObj.AddComponent<FirebaseManager>();
            DontDestroyOnLoad(firebaseManagerObj);

            if (PlayerPrefs.HasKey("UserEmail"))
            {
                string email = PlayerPrefs.GetString("UserEmail");
                firebaseManager.SetUserEmail(email);
                Debug.Log($"TestTimerManager: Set FirebaseManager email to: {email}");
            }
        }

        string userEmail = firebaseManager.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogWarning("TestTimerManager: User email not set, cannot upload score");
            ShowUploadStatus("Upload skipped: User not logged in", Color.yellow);
            return;
        }

        ShowUploadStatus("Uploading score...", Color.white);

        firebaseManager.UploadTestScore(averageReactionTime, (success) =>
        {
            if (success)
            {
                Debug.Log("TestTimerManager: Score uploaded successfully");
                ShowUploadStatus("Score uploaded successfully!", Color.green);
            }
            else
            {
                Debug.LogError("TestTimerManager: Failed to upload score");
                ShowUploadStatus("Failed to upload score", Color.red);
            }
        });
    }

    private void ShowUploadStatus(string message, Color color)
    {
        if (uploadStatusText != null)
        {
            uploadStatusText.gameObject.SetActive(true);
            uploadStatusText.text = message;
            uploadStatusText.color = color;
        }
    }

    private void OnEndTestClicked()
    {
        Debug.Log("TestTimerManager: End Test button clicked - loading main menu");

        if (ReactionTimeManager.Instance != null)
        {
            ReactionTimeManager.Instance.SaveAverageReactionTime();
        }

        SceneManager.LoadScene("Main menu");
    }

    private void OnRestartClicked()
    {
        Debug.Log("TestTimerManager: Restart button clicked - reloading scene");

        if (mainContentObject != null)
        {
            mainContentObject.SetActive(true);
            Debug.Log("TestTimerManager: MainContent shown again");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool HasTag(string tag)
    {
        try
        {
            GameObject temp = new GameObject();
            temp.tag = tag;
            Destroy(temp);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void AddTagIfNotExists(string tag)
    {
        if (!HasTag(tag))
        {
            Debug.LogWarning($"The tag '{tag}' doesn't exist! Please add it in the Unity Editor under Edit > Project Settings > Tags and Layers.");
        }
    }
}
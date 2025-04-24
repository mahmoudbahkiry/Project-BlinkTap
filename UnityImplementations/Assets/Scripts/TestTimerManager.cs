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

    // Public method to check if test is currently active
    public bool IsTestActive()
    {
        return testInProgress;
    }

    private void Awake()
    {
        // Make sure the "Circle" tag exists
        // This is a safeguard, you should still create the tag in the Editor
        AddTagIfNotExists("Circle");

        // Get the CircleSpawner component from the object
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

        // Check for MainContent reference
        if (mainContentObject == null)
        {
            Debug.LogError("MainContent object not assigned in Inspector!");
        }

        // Initialize upload status text
        if (uploadStatusText != null)
        {
            uploadStatusText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Make sure MainContent is visible at start
        if (mainContentObject != null)
        {
            mainContentObject.SetActive(true);
        }

        // Make sure the circle spawner does not spawn at start
        if (circleSpawner != null)
        {
            // Make sure it's enabled as a component but not spawning yet
            circleSpawner.enabled = true;
            circleSpawner.StopSpawning();
            Debug.Log("TestTimerManager: Initialized CircleSpawner (not spawning yet)");
        }

        // Hide results popup at start
        if (resultsPopup != null)
        {
            resultsPopup.SetActive(false);
        }
        else
        {
            Debug.LogError("Results popup not assigned in Inspector!");
        }

        // Initialize timer text
        if (timerText != null)
        {
            timerText.text = $"Time: {testDuration:00.0}";
        }

        // Setup button listeners
        if (endTestButton != null)
        {
            endTestButton.onClick.AddListener(OnEndTestClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        // Start countdown
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        Debug.Log("TestTimerManager: Starting countdown sequence");

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
                Debug.Log("TestTimerManager: Countdown - " + i);
            }
            yield return new WaitForSeconds(1f);
        }

        // Show START! text
        if (countdownText != null)
        {
            countdownText.text = "START!";
            Debug.Log("TestTimerManager: Countdown - START!");
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
        Debug.Log("TestTimerManager: StartTest called - Starting the test now");

        testInProgress = true;
        testTimer = testDuration;

        // Reset reaction time manager data
        if (ReactionTimeManager.Instance != null)
        {
            ReactionTimeManager.Instance.ResetReactionTimes();
        }
        else
        {
            Debug.LogError("ReactionTimeManager instance not found!");
        }

        // Enable circle spawner
        if (circleSpawner != null)
        {
            // First make sure it's stopped (redundant, but safe)
            circleSpawner.StopSpawning();

            // Now start it
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
            // Update timer
            testTimer -= Time.deltaTime;

            // Update timer display
            if (timerText != null)
            {
                timerText.text = $"Time: {testTimer:00.0}";
            }

            // Check if test is over
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

        // Update timer display to show 0
        if (timerText != null)
        {
            timerText.text = "Time: 00.0";
        }

        // Immediately disable circle spawner - double check this happens
        if (circleSpawner != null)
        {
            // First stop spawning
            circleSpawner.StopSpawning();

            // Then disable the component
            circleSpawner.enabled = false;
            Debug.Log("TestTimerManager: Circle spawner explicitly STOPPED and disabled");
        }
        else
        {
            Debug.LogError("CircleSpawner not found - cannot properly stop it!");
        }

        // Safer way to handle remaining circles
        try
        {
            // Only destroy circles if the tag exists
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

        // Hide countdown text if visible
        if (countdownText != null && countdownText.gameObject.activeSelf)
        {
            countdownText.gameObject.SetActive(false);
        }

        // Show results popup with average reaction time
        ShowResultsPopup();
    }

    private void ShowResultsPopup()
    {
        // Hide MainContent
        if (mainContentObject != null)
        {
            mainContentObject.SetActive(false);
            Debug.Log("TestTimerManager: MainContent hidden");
        }

        // Show results popup
        if (resultsPopup != null)
        {
            resultsPopup.SetActive(true);
            Debug.Log("TestTimerManager: Results popup displayed");

            // Calculate and display average reaction time
            if (averageReactionTimeText != null && ReactionTimeManager.Instance != null)
            {
                lastAverageReactionTime = ReactionTimeManager.Instance.AverageReactionTime;
                averageReactionTimeText.text = $"Average Reaction Time: {lastAverageReactionTime:F0} ms";
                Debug.Log($"TestTimerManager: Average reaction time: {lastAverageReactionTime:F0} ms");

                // Upload score to Firebase
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
        // Try to find the FirebaseManager
        FirebaseManager firebaseManager = FindObjectOfType<FirebaseManager>();

        // If not found, create one
        if (firebaseManager == null)
        {
            Debug.Log("TestTimerManager: FirebaseManager not found, creating one...");
            GameObject firebaseManagerObj = new GameObject("FirebaseManager");
            firebaseManager = firebaseManagerObj.AddComponent<FirebaseManager>();
            DontDestroyOnLoad(firebaseManagerObj);

            // Try to get email from PlayerPrefs
            if (PlayerPrefs.HasKey("UserEmail"))
            {
                string email = PlayerPrefs.GetString("UserEmail");
                firebaseManager.SetUserEmail(email);
                Debug.Log($"TestTimerManager: Set FirebaseManager email to: {email}");
            }
        }

        // Check if user email is set
        string userEmail = firebaseManager.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogWarning("TestTimerManager: User email not set, cannot upload score");
            ShowUploadStatus("Upload skipped: User not logged in", Color.yellow);
            return;
        }

        // Show uploading status
        ShowUploadStatus("Uploading score...", Color.white);

        // Upload the score
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

        // Save the average reaction time to PlayerPrefs before returning to main menu
        if (ReactionTimeManager.Instance != null)
        {
            ReactionTimeManager.Instance.SaveAverageReactionTime();
        }

        // Load the main menu scene
        SceneManager.LoadScene("Main menu");
    }

    private void OnRestartClicked()
    {
        Debug.Log("TestTimerManager: Restart button clicked - reloading scene");

        // Show MainContent if we're not reloading the scene (this shouldn't happen normally)
        if (mainContentObject != null)
        {
            mainContentObject.SetActive(true);
            Debug.Log("TestTimerManager: MainContent shown again");
        }

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Helper method to check if a tag exists
    private bool HasTag(string tag)
    {
        try
        {
            // Unity will throw an exception if we try to access a tag that doesn't exist
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

    // Helper method to programmatically add a tag if it doesn't exist
    // Note: This isn't a full solution as runtime tag creation isn't supported
    // The proper fix is to add the tag in the Unity Editor
    private void AddTagIfNotExists(string tag)
    {
        // This doesn't actually work at runtime, but we'll keep it as a reminder
        // that you need to add the tag in the Unity Editor
        if (!HasTag(tag))
        {
            Debug.LogWarning($"The tag '{tag}' doesn't exist! Please add it in the Unity Editor under Edit > Project Settings > Tags and Layers.");
        }
    }
}
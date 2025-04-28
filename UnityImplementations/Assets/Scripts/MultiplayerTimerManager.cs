using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MultiplayerTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float testDuration = 30f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject mainContentObject;

    [Header("Spawner References")]
    [SerializeField] private MultiplayerCircleSpawner player1CircleSpawner;
    [SerializeField] private MultiplayerCircleSpawner player2CircleSpawner;

    [Header("Results Handling")]
    [SerializeField] private MultiplayerManager multiplayerManager;

    private bool testInProgress = false;
    private float testTimer;

    public bool IsTestActive()
    {
        return testInProgress;
    }

    private void Awake()
    {
        if (timerText == null)
        {
            Debug.LogError("Timer Text not assigned in Inspector!");
        }

        if (mainContentObject == null)
        {
            Debug.LogError("MainContent object not assigned in Inspector!");
        }

        if (player1CircleSpawner == null || player2CircleSpawner == null)
        {
            Debug.LogError("One or both player circle spawners not assigned in Inspector!");
        }

        if (multiplayerManager == null)
        {
            multiplayerManager = FindObjectOfType<MultiplayerManager>();
            if (multiplayerManager == null)
            {
                Debug.LogError("MultiplayerManager not found in scene!");
            }
        }
    }

    private void Start()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {testDuration:00.0}";
        }

        StopCircleSpawners();

        testInProgress = false;
    }

    public void StartTest()
    {
        Debug.Log("MultiplayerTimerManager: StartTest called - Starting the test now");

        testInProgress = true;
        testTimer = testDuration;

        StartCircleSpawners();
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
                Debug.Log("MultiplayerTimerManager: Timer reached zero - ending test");
                EndTest();
            }
        }
    }

    private void EndTest()
    {
        Debug.Log("MultiplayerTimerManager: EndTest called - Test is now over");
        testInProgress = false;

        if (timerText != null)
        {
            timerText.text = "Time: 00.0";
        }

        StopCircleSpawners();

        DestroyRemainingCircles();

        if (multiplayerManager != null)
        {
            multiplayerManager.OnTestCompleted();
        }
    }

    private void StartCircleSpawners()
    {
        if (player1CircleSpawner != null)
        {
            player1CircleSpawner.enabled = true;
            player1CircleSpawner.StartSpawning();
            Debug.Log("MultiplayerTimerManager: Player 1 circle spawner started");
        }

        if (player2CircleSpawner != null)
        {
            player2CircleSpawner.enabled = true;
            player2CircleSpawner.StartSpawning();
            Debug.Log("MultiplayerTimerManager: Player 2 circle spawner started");
        }
    }

    private void StopCircleSpawners()
    {
        if (player1CircleSpawner != null)
        {
            player1CircleSpawner.StopSpawning();
            Debug.Log("MultiplayerTimerManager: Player 1 circle spawner stopped");
        }

        if (player2CircleSpawner != null)
        {
            player2CircleSpawner.StopSpawning();
            Debug.Log("MultiplayerTimerManager: Player 2 circle spawner stopped");
        }
    }

    private void DestroyRemainingCircles()
    {
        try
        {
            GameObject[] remainingCircles = GameObject.FindGameObjectsWithTag("Circle");
            foreach (GameObject circle in remainingCircles)
            {
                Destroy(circle);
            }
            Debug.Log($"MultiplayerTimerManager: Destroyed {remainingCircles.Length} remaining circles");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("MultiplayerTimerManager: Error destroying circles: " + e.Message);
        }
    }
}
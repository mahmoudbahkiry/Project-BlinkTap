using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CircleBehavior : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float lifetime = 3f;

    private float spawnTime;
    private bool destroyed = false;
    private int currentTaps = 0;
    private int requiredTaps = 1;
    private Image circleImage;
    private TestTimerManager testManager;
    private bool tagSet = false;

    private void Start()
    {
        spawnTime = Time.time;

        Destroy(gameObject, lifetime);

        circleImage = GetComponent<Image>();

        // Find the test manager once at start
        testManager = FindObjectOfType<TestTimerManager>();
        if (testManager == null)
        {
            Debug.LogWarning("CircleBehavior: TestTimerManager not found in scene!");
        }

        // Try to set the tag, but don't crash if it doesn't exist
        try
        {
            gameObject.tag = "Circle";
            tagSet = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("CircleBehavior: Cannot set tag 'Circle': " + e.Message);
            tagSet = false;
        }

        if (circleImage != null)
        {
            Color circleColor = circleImage.color;

            if (IsColorSimilar(circleColor, Color.red))
            {
                requiredTaps = 1;
            }
            else if (IsColorSimilar(circleColor, Color.blue))
            {
                requiredTaps = 2;
            }
            else if (IsColorSimilar(circleColor, Color.green))
            {
                requiredTaps = 3;
            }
            else
            {
                requiredTaps = 1;
            }

            Debug.Log($"CircleBehavior: Circle spawned with color {circleColor}, requiring {requiredTaps} taps");
        }
    }

    private bool IsColorSimilar(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // First check if we're already destroyed
        if (destroyed)
        {
            return;
        }

        // Check if test is active - ignore clicks if not in progress
        // Use cached reference to test manager or try to find it if null
        if (testManager == null)
        {
            testManager = FindObjectOfType<TestTimerManager>();
        }

        bool testActive = testManager != null && testManager.IsTestActive();

        // If test is not active, ignore click
        if (!testActive)
        {
            Debug.Log("CircleBehavior: Click ignored - test not active");
            return;
        }

        // Process the tap
        currentTaps++;
        Debug.Log($"CircleBehavior: Circle tapped! Current taps: {currentTaps}/{requiredTaps}");

        if (currentTaps >= requiredTaps)
        {
            destroyed = true;

            float reactionTimeInSeconds = Time.time - spawnTime;
            int reactionTimeInMS = Mathf.RoundToInt(reactionTimeInSeconds * 1000);

            if (ReactionTimeManager.Instance != null)
            {
                ReactionTimeManager.Instance.AddReactionTime(reactionTimeInMS);
                Debug.Log($"CircleBehavior: Circle completed with {requiredTaps} taps! Reaction time: {reactionTimeInMS} ms");
            }
            else
            {
                Debug.LogWarning("CircleBehavior: ReactionTimeManager not found - couldn't record reaction time");
            }

            Destroy(gameObject);
        }
        else
        {
            if (circleImage != null)
            {
                float darkenAmount = 0.2f * currentTaps / requiredTaps;
                circleImage.color = new Color(
                    circleImage.color.r - darkenAmount,
                    circleImage.color.g - darkenAmount,
                    circleImage.color.b - darkenAmount,
                    circleImage.color.a
                );
            }
        }
    }
}
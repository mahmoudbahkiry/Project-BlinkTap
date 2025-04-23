using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add UI namespace
using UnityEngine.EventSystems; // Required for event handling

public class CircleBehavior : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float lifetime = 3f;

    private float spawnTime;
    private bool destroyed = false;
    private int currentTaps = 0;
    private int requiredTaps = 1;
    private Image circleImage;

    private void Start()
    {
        // Record spawn time
        spawnTime = Time.time;

        // Destroy the circle after its lifetime
        Destroy(gameObject, lifetime);

        // Get the Image component
        circleImage = GetComponent<Image>();

        // Set required taps based on color
        if (circleImage != null)
        {
            Color circleColor = circleImage.color;

            // Check color and set required taps
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
                requiredTaps = 1; // Default for any other colors
            }

            Debug.Log($"Circle spawned with color {circleColor}, requiring {requiredTaps} taps");
        }
    }

    // Helper method to compare colors with a tolerance
    private bool IsColorSimilar(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    // Implement the OnPointerClick method from IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!destroyed)
        {
            currentTaps++;
            Debug.Log($"Circle tapped! Current taps: {currentTaps}/{requiredTaps}");

            // Only destroy the circle and record reaction time when we reach the required taps
            if (currentTaps >= requiredTaps)
            {
                destroyed = true;

                // Calculate reaction time in milliseconds
                float reactionTimeInSeconds = Time.time - spawnTime;
                int reactionTimeInMS = Mathf.RoundToInt(reactionTimeInSeconds * 1000);

                // Pass the reaction time to the ReactionTimeManager
                ReactionTimeManager.Instance.AddReactionTime(reactionTimeInMS);

                // Log for debugging
                Debug.Log($"Circle completed with {requiredTaps} taps! Reaction time: {reactionTimeInMS} ms");

                // Destroy the circle
                Destroy(gameObject);
            }
            else
            {
                // Visual feedback for partial taps (optional)
                if (circleImage != null)
                {
                    // Slightly darken the circle to indicate progress
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
}
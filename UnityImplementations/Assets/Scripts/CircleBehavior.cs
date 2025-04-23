using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add UI namespace
using UnityEngine.EventSystems; // Required for event handling

public class CircleBehavior : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float lifetime = 3f;

    private float spawnTime;
    private bool clicked = false;

    private void Start()
    {
        // Record spawn time
        spawnTime = Time.time;

        // Destroy the circle after its lifetime
        Destroy(gameObject, lifetime);
    }

    // Implement the OnPointerClick method from IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!clicked)
        {
            clicked = true;

            // Calculate reaction time in milliseconds
            float reactionTimeInSeconds = Time.time - spawnTime;
            int reactionTimeInMS = Mathf.RoundToInt(reactionTimeInSeconds * 1000);

            // Pass the reaction time to the ReactionTimeManager
            ReactionTimeManager.Instance.AddReactionTime(reactionTimeInMS);

            // Log for debugging
            Debug.Log($"Circle clicked! Reaction time: {reactionTimeInMS} ms");

            // Destroy the circle
            Destroy(gameObject);
        }
    }
}
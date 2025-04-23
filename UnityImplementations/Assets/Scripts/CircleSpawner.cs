using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add UI namespace for Image component

public class CircleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 2f;
    [SerializeField] private float circleSize = 1f;

    [Header("Spawn Area")]
    [SerializeField] private RectTransform spawnAreaRect; // Reference to the RectTransform for spawning

    // Available colors for spawned circles
    private Color[] circleColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green
    };

    private void Start()
    {
        // If no spawn area is assigned, use this object's RectTransform
        if (spawnAreaRect == null)
        {
            spawnAreaRect = GetComponent<RectTransform>();
            if (spawnAreaRect == null)
            {
                Debug.LogError("CircleSpawner needs to be on a UI GameObject with a RectTransform!");
                return;
            }
        }

        // Start spawning circles
        StartCoroutine(SpawnCircles());

        // Debug log to verify the script is running
        Debug.Log("CircleSpawner started - should begin spawning circles");
    }

    private IEnumerator SpawnCircles()
    {
        while (true)
        {
            // Wait for a random time between min and max spawn interval
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Spawn a circle
            SpawnCircle();

            // Debug log to verify circles are being spawned
            Debug.Log("Spawning a new circle");
        }
    }

    private void SpawnCircle()
    {
        // Check if prefab is assigned
        if (circlePrefab == null)
        {
            Debug.LogError("Circle prefab is not assigned to the CircleSpawner!");
            return;
        }

        // Calculate random position within the spawn area (RectTransform)
        float randomX = Random.Range(-spawnAreaRect.rect.width / 2, spawnAreaRect.rect.width / 2);
        float randomY = Random.Range(-spawnAreaRect.rect.height / 2, spawnAreaRect.rect.height / 2);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

        // Instantiate the circle
        GameObject circleObject = Instantiate(circlePrefab, spawnAreaRect);

        // Position the circle within the parent
        RectTransform circleRect = circleObject.GetComponent<RectTransform>();
        if (circleRect != null)
        {
            circleRect.anchoredPosition = new Vector2(randomX, randomY);
        }

        // Set random color
        int colorIndex = Random.Range(0, circleColors.Length);
        Image imageRenderer = circleObject.GetComponent<Image>();
        if (imageRenderer != null)
        {
            imageRenderer.color = circleColors[colorIndex];
        }
        else
        {
            Debug.LogWarning("No Image component found on the circle prefab!");
        }

        // Set size
        if (circleRect != null)
        {
            circleRect.localScale = new Vector3(circleSize, circleSize, 1f);
        }

        // Add the CircleBehavior component
        CircleBehavior circleBehavior = circleObject.AddComponent<CircleBehavior>();
    }
}
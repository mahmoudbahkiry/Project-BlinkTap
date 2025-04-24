using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerCircleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 2f;
    [SerializeField] private float circleSize = 1f;
    [SerializeField] private int playerNumber = 1; // 1 for Player 1, 2 for Player 2

    [Header("Spawn Area")]
    [SerializeField] private RectTransform spawnAreaRect;

    [Header("UI Information")]
    [SerializeField] private bool showDebugLogs = true;

    private Color[] circleColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green
    };

    private Coroutine spawnCoroutine = null;
    private bool isSpawning = false;

    private void Start()
    {
        if (spawnAreaRect == null)
        {
            spawnAreaRect = GetComponent<RectTransform>();
            if (spawnAreaRect == null)
            {
                Debug.LogError($"Player {playerNumber} CircleSpawner needs to be on a UI GameObject with a RectTransform!");
                return;
            }
        }

        // Explicitly disable spawning at start
        isSpawning = false;
        spawnCoroutine = null;

        Debug.Log($"Player {playerNumber} CircleSpawner initialized - explicitly NOT spawning at start");
    }

    private void OnDisable()
    {
        // Make sure to stop when disabled
        StopSpawning();
    }

    private void OnDestroy()
    {
        // Make sure to stop when destroyed
        StopSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnCircles());
            Debug.Log($"Player {playerNumber} circle spawning STARTED - Spawner:" + gameObject.name);
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        isSpawning = false;
        Debug.Log($"Player {playerNumber} circle spawning STOPPED - Spawner:" + gameObject.name);
    }

    private IEnumerator SpawnCircles()
    {
        Debug.Log($"Player {playerNumber} SpawnCircles coroutine started");
        while (isSpawning) // Only continue if spawning is active
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Double-check we're still supposed to be spawning
            if (isSpawning && this.enabled && this.gameObject.activeInHierarchy)
            {
                SpawnCircle();
                Debug.Log($"Spawned a Player {playerNumber} circle - isSpawning: " + isSpawning);
            }
            else
            {
                Debug.Log($"Skipped spawning for Player {playerNumber} - isSpawning: " + isSpawning);
            }
        }
        Debug.Log($"Player {playerNumber} SpawnCircles coroutine ended");
    }

    private void SpawnCircle()
    {
        if (circlePrefab == null)
        {
            Debug.LogError($"Circle prefab is not assigned to the Player {playerNumber} CircleSpawner!");
            return;
        }

        float randomX = Random.Range(-spawnAreaRect.rect.width / 2, spawnAreaRect.rect.width / 2);
        float randomY = Random.Range(-spawnAreaRect.rect.height / 2, spawnAreaRect.rect.height / 2);

        GameObject circleObject = Instantiate(circlePrefab, spawnAreaRect);

        RectTransform circleRect = circleObject.GetComponent<RectTransform>();
        if (circleRect != null)
        {
            circleRect.anchoredPosition = new Vector2(randomX, randomY);
            circleRect.localScale = new Vector3(circleSize, circleSize, 1f);
        }

        int colorIndex = Random.Range(0, circleColors.Length);
        Image imageRenderer = circleObject.GetComponent<Image>();
        if (imageRenderer != null)
        {
            // Ensure the image is set up correctly for UI interaction
            imageRenderer.color = circleColors[colorIndex];
            imageRenderer.raycastTarget = true; // Make sure raycast target is enabled

            // Add a Box Collider 2D if needed for better click detection
            if (circleObject.GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = circleObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(circleRect.rect.width, circleRect.rect.height);
            }

            // Add the multiplayer circle behavior and set player owner
            MultiplayerCircleBehavior circleBehavior = circleObject.GetComponent<MultiplayerCircleBehavior>();
            if (circleBehavior == null)
            {
                circleBehavior = circleObject.AddComponent<MultiplayerCircleBehavior>();
            }

            // Set the player owner
            circleBehavior.PlayerOwner = playerNumber;

            if (showDebugLogs)
            {
                int requiredTaps = 1;
                if (colorIndex == 0) requiredTaps = 1;
                else if (colorIndex == 1) requiredTaps = 2;
                else if (colorIndex == 2) requiredTaps = 3;

                Debug.Log($"Spawned a {GetColorName(colorIndex)} circle for Player {playerNumber} requiring {requiredTaps} taps");
            }
        }
        else
        {
            Debug.LogWarning("No Image component found on the circle prefab!");
        }
    }

    private string GetColorName(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0: return "Red";
            case 1: return "Blue";
            case 2: return "Green";
            default: return "Unknown";
        }
    }
}
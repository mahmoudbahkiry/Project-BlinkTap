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

    // Available colors for spawned circles with their corresponding tap requirements
    private Color[] circleColors = new Color[]
    {
        Color.red,    // 1 tap
        Color.blue,   // 2 taps
        Color.green   // 3 taps
    };

    // Text to display on the UI about tap requirements
    [Header("UI Information")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showInstructionsPanel = true;
    [SerializeField] private Transform canvasTransform; // Reference to the Canvas where UI will be created

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

        // Create instructions panel if enabled
        if (showInstructionsPanel)
        {
            CreateInstructionsPanel();
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

            // Determine required taps based on color
            int requiredTaps = 1; // Default
            if (colorIndex == 0) // Red
                requiredTaps = 1;
            else if (colorIndex == 1) // Blue
                requiredTaps = 2;
            else if (colorIndex == 2) // Green
                requiredTaps = 3;

            if (showDebugLogs)
            {
                Debug.Log($"Spawned a {GetColorName(colorIndex)} circle requiring {requiredTaps} taps");
            }
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

        // Add the CircleBehavior component if it doesn't already exist
        CircleBehavior circleBehavior = circleObject.GetComponent<CircleBehavior>();
        if (circleBehavior == null)
        {
            circleBehavior = circleObject.AddComponent<CircleBehavior>();
        }
    }

    // Helper method to get color name from index
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

    // Create a UI panel showing the tap requirements for each color
    private void CreateInstructionsPanel()
    {
        // Make sure we have a canvas reference
        if (canvasTransform == null)
        {
            Debug.LogWarning("Canvas Transform not assigned for instructions panel");
            return;
        }

        // Create a panel GameObject
        GameObject panelGO = new GameObject("TapInstructionsPanel");
        panelGO.transform.SetParent(canvasTransform, false);

        // Add UI components
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        Image panelImage = panelGO.AddComponent<Image>();

        // Configure panel
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(200, 120);
        panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black

        // Create title text
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panelRect, false);
        Text titleText = titleGO.AddComponent<Text>();
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -5);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleText.text = "Tap Requirements:";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 18;

        // Create instruction entries for each color
        CreateColorInstruction(panelRect, 0, "Red: 1 tap", Color.red);
        CreateColorInstruction(panelRect, 1, "Blue: 2 taps", Color.blue);
        CreateColorInstruction(panelRect, 2, "Green: 3 taps", Color.green);
    }

    // Helper to create a single color instruction entry
    private void CreateColorInstruction(RectTransform parent, int index, string instruction, Color color)
    {
        GameObject entryGO = new GameObject("Instruction" + index);
        entryGO.transform.SetParent(parent, false);

        // Create horizontal layout
        HorizontalLayoutGroup layout = entryGO.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 5, 5);

        RectTransform entryRect = entryGO.GetComponent<RectTransform>();
        entryRect.anchorMin = new Vector2(0, 1);
        entryRect.anchorMax = new Vector2(1, 1);
        entryRect.pivot = new Vector2(0.5f, 1);
        entryRect.anchoredPosition = new Vector2(0, -35 - (index * 25));
        entryRect.sizeDelta = new Vector2(0, 25);

        // Create color sample
        GameObject colorSampleGO = new GameObject("ColorSample");
        colorSampleGO.transform.SetParent(entryRect, false);
        Image sampleImage = colorSampleGO.AddComponent<Image>();
        RectTransform sampleRect = colorSampleGO.GetComponent<RectTransform>();
        sampleRect.sizeDelta = new Vector2(20, 20);
        sampleImage.color = color;

        // Create text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(entryRect, false);
        Text instructionText = textGO.AddComponent<Text>();
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150, 20);
        instructionText.text = instruction;
        instructionText.alignment = TextAnchor.MiddleLeft;
        instructionText.color = Color.white;
        instructionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionText.fontSize = 14;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 2f;
    [SerializeField] private float circleSize = 1f;

    [Header("Spawn Area")]
    [SerializeField] private RectTransform spawnAreaRect;

    private Color[] circleColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green
    };

    [Header("UI Information")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showInstructionsPanel = true;
    [SerializeField] private Transform canvasTransform;

    private Coroutine spawnCoroutine = null;
    private bool isSpawning = false;

    private void Start()
    {
        if (spawnAreaRect == null)
        {
            spawnAreaRect = GetComponent<RectTransform>();
            if (spawnAreaRect == null)
            {
                Debug.LogError("CircleSpawner needs to be on a UI GameObject with a RectTransform!");
                return;
            }
        }

        if (showInstructionsPanel)
        {
            CreateInstructionsPanel();
        }

        isSpawning = false;
        spawnCoroutine = null;

        Debug.Log("CircleSpawner initialized - explicitly NOT spawning at start");
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    private void OnDestroy()
    {
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
            Debug.Log("Circle spawning STARTED - Spawner:" + gameObject.name);
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
        Debug.Log("Circle spawning STOPPED - Spawner:" + gameObject.name);
    }

    private IEnumerator SpawnCircles()
    {
        Debug.Log("SpawnCircles coroutine started");
        while (isSpawning)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            if (isSpawning && this.enabled && this.gameObject.activeInHierarchy)
            {
                SpawnCircle();
                Debug.Log("Spawned a circle - isSpawning: " + isSpawning);
            }
            else
            {
                Debug.Log("Skipped spawning - isSpawning: " + isSpawning);
            }
        }
        Debug.Log("SpawnCircles coroutine ended");
    }

    private void SpawnCircle()
    {
        if (circlePrefab == null)
        {
            Debug.LogError("Circle prefab is not assigned to the CircleSpawner!");
            return;
        }

        float randomX = Random.Range(-spawnAreaRect.rect.width / 2, spawnAreaRect.rect.width / 2);
        float randomY = Random.Range(-spawnAreaRect.rect.height / 2, spawnAreaRect.rect.height / 2);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

        GameObject circleObject = Instantiate(circlePrefab, spawnAreaRect);

        RectTransform circleRect = circleObject.GetComponent<RectTransform>();
        if (circleRect != null)
        {
            circleRect.anchoredPosition = new Vector2(randomX, randomY);
        }

        int colorIndex = Random.Range(0, circleColors.Length);
        Image imageRenderer = circleObject.GetComponent<Image>();
        if (imageRenderer != null)
        {
            imageRenderer.color = circleColors[colorIndex];

            int requiredTaps = 1;
            if (colorIndex == 0)
                requiredTaps = 1;
            else if (colorIndex == 1)
                requiredTaps = 2;
            else if (colorIndex == 2)
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

        if (circleRect != null)
        {
            circleRect.localScale = new Vector3(circleSize, circleSize, 1f);
        }

        CircleBehavior circleBehavior = circleObject.GetComponent<CircleBehavior>();
        if (circleBehavior == null)
        {
            circleBehavior = circleObject.AddComponent<CircleBehavior>();
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

    private void CreateInstructionsPanel()
    {
        if (canvasTransform == null)
        {
            Debug.LogWarning("Canvas Transform not assigned for instructions panel");
            return;
        }

        GameObject panelGO = new GameObject("TapInstructionsPanel");
        panelGO.transform.SetParent(canvasTransform, false);

        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        Image panelImage = panelGO.AddComponent<Image>();

        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(200, 120);
        panelImage.color = new Color(0, 0, 0, 0.7f);

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

        CreateColorInstruction(panelRect, 0, "Red: 1 tap", Color.red);
        CreateColorInstruction(panelRect, 1, "Blue: 2 taps", Color.blue);
        CreateColorInstruction(panelRect, 2, "Green: 3 taps", Color.green);
    }

    private void CreateColorInstruction(RectTransform parent, int index, string instruction, Color color)
    {
        GameObject entryGO = new GameObject("Instruction" + index);
        entryGO.transform.SetParent(parent, false);

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

        GameObject colorSampleGO = new GameObject("ColorSample");
        colorSampleGO.transform.SetParent(entryRect, false);
        Image sampleImage = colorSampleGO.AddComponent<Image>();
        RectTransform sampleRect = colorSampleGO.GetComponent<RectTransform>();
        sampleRect.sizeDelta = new Vector2(20, 20);
        sampleImage.color = color;

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
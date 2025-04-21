using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component ensures that critical UI elements remain visible and properly positioned during runtime.
/// Attach to the Canvas or a parent GameObject.
/// </summary>
public class UIVisibilityEnforcer : MonoBehaviour
{
    [Header("UI Elements to Enforce")]
    [SerializeField] private GameObject multiplayerButton;
    [SerializeField] private GameObject reactionTimePanel;
    [SerializeField] private GameObject challengePanel;

    [Header("References")]
    [SerializeField] private MainMenuManager menuManager;

    // Cache original positions
    private Vector3 rtPanelOriginalPosition;
    private Vector3 challengePanelOriginalPosition;
    private bool initialized = false;

    private void Start()
    {
        InitializeReferences();
        CacheOriginalPositions();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        EnforceUIVisibility();
    }

    void InitializeReferences()
    {
        // Find menuManager if not assigned
        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        // Find MultiplayerButton if not assigned
        if (multiplayerButton == null)
        {
            // Try to get it from the menuManager first
            if (menuManager != null && menuManager.multiplayerButton != null)
            {
                multiplayerButton = menuManager.multiplayerButton.gameObject;
            }
            // Fallback to finding by name
            if (multiplayerButton == null)
            {
                multiplayerButton = GameObject.Find("MultiplayerButton");
            }
        }

        // Find ReactionTimePanel if not assigned
        if (reactionTimePanel == null)
        {
            GameObject rtPanel = GameObject.Find("ReactionTimePanel");
            if (rtPanel != null)
                reactionTimePanel = rtPanel;
        }

        // Find ChallengePanel if not assigned
        if (challengePanel == null)
        {
            GameObject chPanel = GameObject.Find("ChallengePanel");
            if (chPanel != null)
                challengePanel = chPanel;
        }
    }

    void CacheOriginalPositions()
    {
        if (reactionTimePanel != null)
            rtPanelOriginalPosition = reactionTimePanel.transform.localPosition;

        if (challengePanel != null)
            challengePanelOriginalPosition = challengePanel.transform.localPosition;
    }

    void EnforceUIVisibility()
    {
        // Ensure MultiplayerButton is active
        if (multiplayerButton != null && !multiplayerButton.activeSelf)
        {
            multiplayerButton.SetActive(true);
        }

        // Ensure ReactionTimePanel position is maintained
        if (reactionTimePanel != null)
        {
            RectTransform rt = reactionTimePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Keep it anchored properly
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(170, 0);
                rt.sizeDelta = new Vector2(340, 0);
            }
        }

        // Ensure ChallengePanel position is maintained
        if (challengePanel != null)
        {
            RectTransform rt = challengePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Keep it anchored properly at the top
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, -220);
                rt.sizeDelta = new Vector2(0, 400);
            }
        }
    }
}
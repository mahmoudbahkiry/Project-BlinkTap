using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisibilityEnforcer : MonoBehaviour
{
    [Header("UI Elements to Enforce")]
    [SerializeField] private GameObject multiplayerButton;
    [SerializeField] private GameObject reactionTimePanel;
    [SerializeField] private GameObject challengePanel;

    [Header("References")]
    [SerializeField] private MainMenuManager menuManager;

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
        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        if (multiplayerButton == null)
        {
            if (menuManager != null && menuManager.multiplayerButton != null)
            {
                multiplayerButton = menuManager.multiplayerButton.gameObject;
            }
            if (multiplayerButton == null)
            {
                multiplayerButton = GameObject.Find("MultiplayerButton");
            }
        }

        if (reactionTimePanel == null)
        {
            GameObject rtPanel = GameObject.Find("ReactionTimePanel");
            if (rtPanel != null)
                reactionTimePanel = rtPanel;
        }

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
        if (multiplayerButton != null && !multiplayerButton.activeSelf)
        {
            multiplayerButton.SetActive(true);
        }

        if (reactionTimePanel != null)
        {
            RectTransform rt = reactionTimePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(170, 0);
                rt.sizeDelta = new Vector2(340, 0);
            }
        }

        if (challengePanel != null)
        {
            RectTransform rt = challengePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, -220);
                rt.sizeDelta = new Vector2(0, 400);
            }
        }
    }
}
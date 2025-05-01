using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisibilityEnforcer : MonoBehaviour
{
    [Header("UI Elements to Enforce")]
    [SerializeField] private GameObject multiplayerButton;

    [Header("References")]
    [SerializeField] private MainMenuManager menuManager;

    private bool initialized = false;

    private void Start()
    {
        InitializeReferences();
        EnforceUIVisibility();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        if (multiplayerButton != null && !multiplayerButton.activeSelf)
        {
            multiplayerButton.SetActive(true);
        }
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
    }

    void EnforceUIVisibility()
    {
        if (multiplayerButton != null && !multiplayerButton.activeSelf)
        {
            multiplayerButton.SetActive(true);
        }
    }
}
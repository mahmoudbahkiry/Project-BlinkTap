using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeButtonController : MonoBehaviour
{
    [Header("Button Configuration")]
    [SerializeField] private ModeType modeType = ModeType.Solo;
    [SerializeField] private string modeName = "Solo";
    [SerializeField] private string modeDescription = "Train your reflexes";
    [SerializeField] private Sprite modeIcon;

    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button modeButton;

    // References
    private UIElementStyler styler;
    private MainMenuManager menuManager;

    private void Awake()
    {
        // Try to find references if not assigned
        if (modeButton == null)
            modeButton = GetComponent<Button>();

        if (iconImage == null)
            iconImage = transform.Find("IconContainer")?.GetComponentInChildren<Image>();

        if (titleText == null)
            titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();

        if (descriptionText == null)
            descriptionText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        // Find references in scene
        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        // Apply mode configuration
        ApplyModeSettings();

        // Setup button click event
        if (modeButton != null)
        {
            modeButton.onClick.AddListener(OnModeButtonClicked);
        }
    }

    public void ApplyModeSettings()
    {
        // Set icon
        if (iconImage != null && modeIcon != null)
        {
            iconImage.sprite = modeIcon;
        }

        // Set text
        if (titleText != null)
        {
            titleText.text = modeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = modeDescription;
        }

        // Apply styling
        if (styler != null)
        {
            styler.StyleModeButton(modeButton, iconImage, titleText, descriptionText, modeType);
        }
    }

    private void OnModeButtonClicked()
    {
        // Forward event to menu manager
        if (menuManager != null)
        {
            // Call the LoadGameMode method on the MainMenuManager
            Debug.Log("Mode button clicked: " + modeName);
        }
    }

    // Method to configure button externally
    public void Configure(ModeType type, string name, string description, Sprite icon)
    {
        modeType = type;
        modeName = name;
        modeDescription = description;
        modeIcon = icon;

        // If the object is active, apply the settings immediately
        if (gameObject.activeSelf)
        {
            ApplyModeSettings();
        }
    }
}
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

    private UIElementStyler styler;
    private MainMenuManager menuManager;

    private void Awake()
    {
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
        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        if (menuManager == null)
            menuManager = FindObjectOfType<MainMenuManager>();

        string objName = gameObject.name.ToLower();
        if (objName.Contains("multiplayer"))
        {
            modeType = ModeType.Multiplayer;
            modeName = "Multiplayer";
            modeDescription = "Compete with others";

            if (menuManager != null)
            {
                menuManager.multiplayerButton = GetComponent<Button>();
            }
        }
        else if (objName.Contains("solo"))
        {
            modeType = ModeType.Solo;
            modeName = "Solo";
            modeDescription = "Train your reflexes";

            if (menuManager != null)
            {
                menuManager.SetSoloButton(GetComponent<Button>());
            }
        }

        ApplyModeSettings();

        if (modeButton != null)
        {
            modeButton.onClick.AddListener(OnModeButtonClicked);
        }
    }

    public void ApplyModeSettings()
    {
        if (iconImage != null && modeIcon != null)
        {
            iconImage.sprite = modeIcon;
        }

        if (titleText != null)
        {
            titleText.text = modeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = modeDescription;
        }

        if (styler != null)
        {
            styler.StyleModeButton(modeButton, iconImage, titleText, descriptionText, modeType);
        }
    }

    private void OnModeButtonClicked()
    {
        if (menuManager != null)
        {
            Debug.Log("Mode button clicked: " + modeName);

            menuManager.LoadGameMode(modeName);
        }
    }

    public void Configure(ModeType type, string name, string description, Sprite icon)
    {
        modeType = type;
        modeName = name;
        modeDescription = description;
        modeIcon = icon;

        if (gameObject.activeSelf)
        {
            ApplyModeSettings();
        }
    }
}
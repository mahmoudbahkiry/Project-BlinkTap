using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the ProfilePanel UI and functionality
/// </summary>
public class ProfilePanelController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TMP_Dropdown professionDropdown;
    [SerializeField] private Button saveButton;
    [SerializeField] private Image backgroundPanel;

    private MainMenuManager menuManager;

    private void Awake()
    {
        // Try to find references if not assigned
        if (titleText == null)
            titleText = transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();

        if (emailText == null)
            emailText = transform.Find("Content/EmailText")?.GetComponent<TextMeshProUGUI>();

        if (professionDropdown == null)
            professionDropdown = transform.Find("Content/ProfessionDropdown")?.GetComponent<TMP_Dropdown>();

        if (saveButton == null)
            saveButton = transform.Find("Buttons/SaveButton")?.GetComponent<Button>();

        if (backgroundPanel == null)
            backgroundPanel = GetComponent<Image>();
    }

    void Start()
    {
        // Find references
        menuManager = FindObjectOfType<MainMenuManager>();

        // Style the panel
        ApplyStyle();
    }

    void ApplyStyle()
    {
        // Set panel background
        if (backgroundPanel != null)
            backgroundPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Style title
        if (titleText != null)
        {
            titleText.text = "User Profile";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
        }

        // Style email label
        Transform emailLabelTransform = transform.Find("Content/EmailLabel");
        if (emailLabelTransform != null)
        {
            TextMeshProUGUI emailLabel = emailLabelTransform.GetComponent<TextMeshProUGUI>();
            if (emailLabel != null)
            {
                emailLabel.text = "Email:";
                emailLabel.fontSize = 18;
                emailLabel.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        // Style profession label
        Transform professionLabelTransform = transform.Find("Content/ProfessionLabel");
        if (professionLabelTransform != null)
        {
            TextMeshProUGUI professionLabel = professionLabelTransform.GetComponent<TextMeshProUGUI>();
            if (professionLabel != null)
            {
                professionLabel.text = "Profession:";
                professionLabel.fontSize = 18;
                professionLabel.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        // Style save button
        if (saveButton != null)
        {
            // Get button image
            Image saveButtonImage = saveButton.GetComponent<Image>();
            if (saveButtonImage != null)
                saveButtonImage.color = new Color(0f, 0.8f, 1f); // BlinkTap cyan

            // Get button text
            TextMeshProUGUI saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (saveButtonText != null)
            {
                saveButtonText.text = "SAVE";
                saveButtonText.color = Color.black;
                saveButtonText.fontStyle = FontStyles.Bold;
            }
        }
    }
}
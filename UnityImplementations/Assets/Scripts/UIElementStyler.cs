using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIElementStyler handles the styling of UI elements 
/// such as buttons, panels and other custom UI components
/// </summary>
public class UIElementStyler : MonoBehaviour
{
    [Header("Mode Button Styling")]
    [SerializeField] private Color soloModeColor = new Color(0.1254902f, 0.2941177f, 0.6f);
    [SerializeField] private Color multiplayerModeColor = new Color(0.6f, 0.1254902f, 0.1882353f);

    [Header("Background Color")]
    [SerializeField] private Color backgroundColor = new Color(0.09803922f, 0.09803922f, 0.09803922f);
    [SerializeField] private Color headerPanelColor = new Color(0.1254902f, 0.1254902f, 0.1254902f);

    [Header("Text Colors")]
    [SerializeField] private Color primaryTextColor = Color.white;
    [SerializeField] private Color secondaryTextColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color accentTextColor = new Color(0f, 0.8f, 1f); // BlinkTap cyan color

    [Header("Accent Color for Stats")]
    [SerializeField] private Color positiveChangeColor = new Color(0.2f, 0.8f, 0.4f);
    [SerializeField] private Color negativeChangeColor = new Color(0.8f, 0.2f, 0.2f);

    /// <summary>
    /// Apply mode button style to a button with an icon and label
    /// </summary>
    public void StyleModeButton(Button button, Image iconImage, TextMeshProUGUI titleText,
                               TextMeshProUGUI descriptionText, ModeType mode)
    {
        if (button == null || iconImage == null) return;

        // Get background image component
        Image backgroundImage = button.GetComponent<Image>();
        if (backgroundImage == null) return;

        // Set background color based on mode type
        Color backgroundColor = GetModeColor(mode);
        backgroundImage.color = backgroundColor;

        // Style texts
        if (titleText != null)
        {
            titleText.color = primaryTextColor;
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
        }

        if (descriptionText != null)
        {
            descriptionText.color = secondaryTextColor;
            descriptionText.fontSize = 18;
        }
    }

    /// <summary>
    /// Get color for specific mode
    /// </summary>
    public Color GetModeColor(ModeType mode)
    {
        switch (mode)
        {
            case ModeType.Solo:
                return soloModeColor;
            case ModeType.Multiplayer:
                return multiplayerModeColor;
            default:
                return soloModeColor;
        }
    }

    /// <summary>
    /// Style challenge panel
    /// </summary>
    public void StyleChallengePanel(Image backgroundPanel, TextMeshProUGUI titleText,
                                   TextMeshProUGUI bestScoreText, TextMeshProUGUI playersCompletedText,
                                   Button startButton)
    {
        if (backgroundPanel == null) return;

        // Style the panel
        backgroundPanel.color = soloModeColor; // Using the solo mode color for challenge panel

        // Style texts
        if (titleText != null)
        {
            titleText.color = primaryTextColor;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
        }

        if (bestScoreText != null)
        {
            bestScoreText.color = primaryTextColor;
            bestScoreText.fontSize = 22;
        }

        if (playersCompletedText != null)
        {
            playersCompletedText.color = secondaryTextColor;
            playersCompletedText.fontSize = 18;
        }

        // Style start button
        if (startButton != null)
        {
            Image buttonImage = startButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = accentTextColor;
            }

            TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = Color.black;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.fontSize = 24;
            }
        }
    }

    /// <summary>
    /// Style stats panel for reaction time
    /// </summary>
    public void StyleStatsPanel(Image backgroundPanel, TextMeshProUGUI titleText,
                               TextMeshProUGUI valueText, TextMeshProUGUI changeText,
                               bool isPositiveChange = true)
    {
        if (backgroundPanel == null) return;

        // Style the panel
        backgroundPanel.color = headerPanelColor;

        // Style texts
        if (titleText != null)
        {
            titleText.color = secondaryTextColor;
            titleText.fontSize = 22;
        }

        if (valueText != null)
        {
            valueText.color = accentTextColor;
            valueText.fontSize = 46;
            valueText.fontStyle = FontStyles.Bold;
        }

        if (changeText != null)
        {
            changeText.color = isPositiveChange ? positiveChangeColor : negativeChangeColor;
            changeText.fontSize = 18;
        }
    }
}

/// <summary>
/// Mode types for styling
/// </summary>
public enum ModeType
{
    Solo,
    Multiplayer
}
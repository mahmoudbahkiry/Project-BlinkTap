using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsPanelController : MonoBehaviour
{
    [Header("Panel Type")]
    [SerializeField] private StatsType statsType = StatsType.ReactionTime;

    [Header("UI Components")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI changeText;

    // References
    private UIElementStyler styler;

    private void Awake()
    {
        // Try to find references if not assigned
        if (backgroundPanel == null)
            backgroundPanel = GetComponent<Image>();

        if (titleText == null)
            titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();

        if (valueText == null)
            valueText = transform.Find("Value")?.GetComponent<TextMeshProUGUI>();

        if (changeText == null)
            changeText = transform.Find("Change")?.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        // Find references in scene
        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        // Apply initial settings
        SetupStatPanel();
    }

    public void SetupStatPanel()
    {
        // Set title based on stats type
        if (titleText != null)
        {
            titleText.text = GetStatTitle();
        }

        // Apply styling
        if (styler != null)
        {
            styler.StyleStatsPanel(
                backgroundPanel,
                titleText,
                valueText,
                changeText,
                IsPositiveChange());
        }
    }

    // Update stats value
    public void UpdateValue(string value, string change)
    {
        if (valueText != null)
        {
            valueText.text = value;
        }

        if (changeText != null)
        {
            bool isPositive = change.StartsWith("+") || change.StartsWith("↑");
            changeText.text = change;

            // Update color based on change direction
            if (styler != null)
            {
                styler.StyleStatsPanel(
                    backgroundPanel,
                    titleText,
                    valueText,
                    changeText,
                    isPositive);
            }
        }
    }

    // Helper method to get stat title
    private string GetStatTitle()
    {
        switch (statsType)
        {
            case StatsType.ReactionTime:
                return "Reaction Time";
            case StatsType.GlobalRank:
                return "Global Rank";
            case StatsType.BestScore:
                return "Best Score";
            case StatsType.GamesPlayed:
                return "Games Played";
            default:
                return "Stat";
        }
    }

    // Helper method to determine if change is positive
    private bool IsPositiveChange()
    {
        if (changeText == null)
            return true;

        string text = changeText.text;
        return text.StartsWith("+") || text.StartsWith("↑");
    }
}

/// <summary>
/// Types of stats panels
/// </summary>
public enum StatsType
{
    ReactionTime,
    GlobalRank,
    BestScore,
    GamesPlayed
}
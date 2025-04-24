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
        Debug.Log($"StatsPanelController: Awake called on {gameObject.name}");

        // Try to find references if not assigned
        if (backgroundPanel == null)
        {
            backgroundPanel = GetComponent<Image>();
            Debug.Log($"StatsPanelController: Found backgroundPanel: {(backgroundPanel != null ? "Yes" : "No")}");
        }

        if (titleText == null)
        {
            Transform titleTransform = transform.Find("Title");
            titleText = titleTransform?.GetComponent<TextMeshProUGUI>();
            Debug.Log($"StatsPanelController: Found Title transform: {(titleTransform != null ? "Yes" : "No")}, with TextMeshProUGUI: {(titleText != null ? "Yes" : "No")}");
        }

        if (valueText == null)
        {
            Transform valueTransform = transform.Find("ValueText");
            valueText = valueTransform?.GetComponent<TextMeshProUGUI>();
            Debug.Log($"StatsPanelController: Found ValueText transform: {(valueTransform != null ? "Yes" : "No")}, with TextMeshProUGUI: {(valueText != null ? "Yes" : "No")}");

            // If still null, try searching for "Value" instead
            if (valueText == null)
            {
                Transform alternateValueTransform = transform.Find("Value");
                valueText = alternateValueTransform?.GetComponent<TextMeshProUGUI>();
                Debug.Log($"StatsPanelController: Found alternate Value transform: {(alternateValueTransform != null ? "Yes" : "No")}, with TextMeshProUGUI: {(valueText != null ? "Yes" : "No")}");
            }

            // Last resort - look for any TextMeshProUGUI components that might be the value text
            if (valueText == null)
            {
                // Log the full hierarchy for debugging
                Debug.Log("StatsPanelController: Dumping full GameObject hierarchy:");
                PrintHierarchy(transform, 0);

                // Try to find any TextMeshProUGUI that might be our value
                TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                Debug.Log($"StatsPanelController: Found {allTexts.Length} TextMeshProUGUI components in children");
                for (int i = 0; i < allTexts.Length; i++)
                {
                    Debug.Log($"  [#{i}] GameObject: {allTexts[i].gameObject.name}, Text: {allTexts[i].text}");
                }
            }
        }

        if (changeText == null)
        {
            Transform changeTransform = transform.Find("Change");
            changeText = changeTransform?.GetComponent<TextMeshProUGUI>();
            Debug.Log($"StatsPanelController: Found Change transform: {(changeTransform != null ? "Yes" : "No")}, with TextMeshProUGUI: {(changeText != null ? "Yes" : "No")}");
        }
    }

    // Helper method to print the full hierarchy for debugging
    private void PrintHierarchy(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}GameObject: {t.gameObject.name}, Components: {string.Join(", ", GetComponentNames(t))}");

        foreach (Transform child in t)
        {
            PrintHierarchy(child, depth + 1);
        }
    }

    private string[] GetComponentNames(Transform t)
    {
        Component[] components = t.GetComponents<Component>();
        string[] names = new string[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            names[i] = components[i].GetType().Name;
        }
        return names;
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
        Debug.Log($"StatsPanelController: UpdateValue called with value={value}, change={change}");

        if (valueText != null)
        {
            valueText.text = value;
            Debug.Log($"StatsPanelController: valueText updated to '{value}'");
        }
        else
        {
            Debug.LogError("StatsPanelController: valueText is null, cannot update!");
        }

        if (changeText != null)
        {
            bool isPositive = change.StartsWith("+") || change.StartsWith("↑");
            changeText.text = change;
            Debug.Log($"StatsPanelController: changeText updated to '{change}'");

            // Update color based on change direction
            if (styler != null)
            {
                styler.StyleStatsPanel(
                    backgroundPanel,
                    titleText,
                    valueText,
                    changeText,
                    isPositive);
                Debug.Log("StatsPanelController: Styling applied to panel");
            }
            else
            {
                Debug.LogWarning("StatsPanelController: styler is null, cannot apply styling");
            }
        }
        else
        {
            Debug.LogWarning("StatsPanelController: changeText is null");
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

    // Make valueText accessible for direct updates
    public TextMeshProUGUI ValueTextComponent
    {
        get { return valueText; }
    }

    // Method to directly update the value text for debugging
    public void UpdateValueTextDirectly(string text)
    {
        Debug.Log($"StatsPanelController: Direct update of ValueText to '{text}'");

        if (valueText != null)
        {
            valueText.text = text;
            Debug.Log($"StatsPanelController: ValueText directly updated to '{text}'");
        }
        else
        {
            Debug.LogError("StatsPanelController: Cannot update ValueText (null reference)");

            // Try to find it again as a last resort
            if (FindAndAssignValueText())
            {
                valueText.text = text;
                Debug.Log($"StatsPanelController: ValueText found and updated to '{text}'");
            }
        }
    }

    // Helper method to find and assign the valueText component
    private bool FindAndAssignValueText()
    {
        // Try different possible names
        string[] possibleNames = new string[] { "ValueText", "Value" };

        foreach (string name in possibleNames)
        {
            Transform valueTransform = transform.Find(name);
            if (valueTransform != null)
            {
                valueText = valueTransform.GetComponent<TextMeshProUGUI>();
                if (valueText != null)
                {
                    Debug.Log($"StatsPanelController: Found valueText with name '{name}'");
                    return true;
                }
            }
        }

        // Last resort - find first TextMeshProUGUI that isn't title or change
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != titleText && text != changeText)
            {
                valueText = text;
                Debug.Log($"StatsPanelController: Using {text.gameObject.name} as valueText (last resort)");
                return true;
            }
        }

        Debug.LogError("StatsPanelController: Failed to find valueText by any method");
        return false;
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
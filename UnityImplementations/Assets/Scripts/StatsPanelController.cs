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

    private UIElementStyler styler;

    private RectTransform myRectTransform;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;
    private Color originalBackgroundColor;
    private bool initialValuesStored = false;

    private void StoreInitialValues()
    {
        if (initialValuesStored) return;

        myRectTransform = transform as RectTransform;
        if (myRectTransform != null)
        {
            originalAnchorMin = myRectTransform.anchorMin;
            originalAnchorMax = myRectTransform.anchorMax;
            originalSizeDelta = myRectTransform.sizeDelta;
            originalAnchoredPosition = myRectTransform.anchoredPosition;
            originalScale = myRectTransform.localScale;
        }

        if (backgroundPanel != null)
        {
            originalBackgroundColor = backgroundPanel.color;
        }

        initialValuesStored = true;
    }

    private void Awake()
    {
        Debug.Log($"StatsPanelController: Awake called on {gameObject.name}");

        StoreInitialValues();

        if (backgroundPanel == null)
        {
            backgroundPanel = GetComponent<Image>();
            Debug.Log($"StatsPanelController: Found backgroundPanel: {(backgroundPanel != null ? "Yes" : "No")}");

            if (backgroundPanel != null && !initialValuesStored)
            {
                originalBackgroundColor = backgroundPanel.color;
            }
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

            if (valueText == null)
            {
                Transform alternateValueTransform = transform.Find("Value");
                valueText = alternateValueTransform?.GetComponent<TextMeshProUGUI>();
                Debug.Log($"StatsPanelController: Found alternate Value transform: {(alternateValueTransform != null ? "Yes" : "No")}, with TextMeshProUGUI: {(valueText != null ? "Yes" : "No")}");
            }

            if (valueText == null)
            {
                Debug.Log("StatsPanelController: Dumping full GameObject hierarchy:");
                PrintHierarchy(transform, 0);

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
        StoreInitialValues();

        if (styler == null)
            styler = FindObjectOfType<UIElementStyler>();

        SetupStatPanel();
    }

    void Update()
    {
        MaintainOriginalAppearance();
    }

    private void MaintainOriginalAppearance()
    {
        if (!initialValuesStored) return;

        if (myRectTransform != null)
        {
            myRectTransform.anchorMin = originalAnchorMin;
            myRectTransform.anchorMax = originalAnchorMax;
            myRectTransform.sizeDelta = originalSizeDelta;
            myRectTransform.anchoredPosition = originalAnchoredPosition;
            myRectTransform.localScale = originalScale;
        }

        if (backgroundPanel != null)
        {
            backgroundPanel.color = originalBackgroundColor;
        }
    }

    public void SetupStatPanel()
    {
        if (titleText != null)
        {
            titleText.text = GetStatTitle();
        }

        if (styler != null && !initialValuesStored)
        {
            Color origColor = backgroundPanel != null ? backgroundPanel.color : Color.white;

            styler.StyleStatsPanel(
                backgroundPanel,
                titleText,
                valueText,
                changeText,
                IsPositiveChange());

            StoreInitialValues();
        }
    }

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

        }
        else
        {
            Debug.LogWarning("StatsPanelController: changeText is null");
        }
    }

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

    private bool IsPositiveChange()
    {
        if (changeText == null)
            return true;

        string text = changeText.text;
        return text.StartsWith("+") || text.StartsWith("↑");
    }

    public TextMeshProUGUI ValueTextComponent
    {
        get { return valueText; }
    }

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

            if (FindAndAssignValueText())
            {
                valueText.text = text;
                Debug.Log($"StatsPanelController: ValueText found and updated to '{text}'");
            }
        }
    }

    private bool FindAndAssignValueText()
    {
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

public enum StatsType
{
    ReactionTime,
    GlobalRank,
    BestScore,
    GamesPlayed
}
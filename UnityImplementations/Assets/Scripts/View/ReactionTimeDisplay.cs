using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionTimeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI averageTimeText;
    [SerializeField] private TextMeshProUGUI totalTapsText;
    [SerializeField] private TextMeshProUGUI lastReactionTimeText;

    private int lastReactionTime = 0;

    void Start()
    {
        ResetLastReactionTimeDisplay();
    }

    void Update()
    {
        if (ReactionTimeManager.Instance != null)
        {
            if (averageTimeText != null)
            {
                averageTimeText.text = $"Average: {ReactionTimeManager.Instance.AverageReactionTime:F0} ms";
            }

            if (totalTapsText != null)
            {
                totalTapsText.text = $"Taps: {ReactionTimeManager.Instance.TotalTaps}";
            }

            List<int> allTimes = ReactionTimeManager.Instance.AllReactionTimes;
            if (allTimes.Count > 0 && allTimes.Count > lastReactionTime)
            {
                lastReactionTime = allTimes.Count;
                if (lastReactionTimeText != null)
                {
                    lastReactionTimeText.text = $"Last: {allTimes[allTimes.Count - 1]} ms";
                }
            }
            else if (allTimes.Count == 0 && lastReactionTime > 0)
            {
                ResetLastReactionTimeDisplay();
            }
        }
    }

    public void ResetLastReactionTimeDisplay()
    {
        lastReactionTime = 0;
        if (lastReactionTimeText != null)
        {
            lastReactionTimeText.text = "Last: 0 ms";
        }
    }
}
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

    // Update is called once per frame
    void Update()
    {
        if (ReactionTimeManager.Instance != null)
        {
            // Update average reaction time
            if (averageTimeText != null)
            {
                averageTimeText.text = $"Average: {ReactionTimeManager.Instance.AverageReactionTime:F0} ms";
            }

            // Update total taps
            if (totalTapsText != null)
            {
                totalTapsText.text = $"Taps: {ReactionTimeManager.Instance.TotalTaps}";
            }

            // Get latest reaction time (if there's a new one)
            List<int> allTimes = ReactionTimeManager.Instance.AllReactionTimes;
            if (allTimes.Count > 0 && allTimes.Count > lastReactionTime)
            {
                lastReactionTime = allTimes.Count;
                if (lastReactionTimeText != null)
                {
                    lastReactionTimeText.text = $"Last: {allTimes[allTimes.Count - 1]} ms";
                }
            }
        }
    }
}
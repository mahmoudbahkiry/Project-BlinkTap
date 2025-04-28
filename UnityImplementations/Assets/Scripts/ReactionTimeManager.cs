using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionTimeManager : MonoBehaviour
{
    public static ReactionTimeManager Instance { get; private set; }

    private List<int> reactionTimes = new List<int>();

    public float AverageReactionTime
    {
        get
        {
            if (reactionTimes.Count == 0) return 0;

            int sum = 0;
            foreach (int time in reactionTimes)
            {
                sum += time;
            }
            return (float)sum / reactionTimes.Count;
        }
    }

    public List<int> AllReactionTimes => new List<int>(reactionTimes);

    public int TotalTaps => reactionTimes.Count;

    public const string LAST_REACTION_TIME_KEY = "LastAverageReactionTime";
    public const string HAS_REACTION_TIME_DATA_KEY = "HasReactionTimeData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddReactionTime(int milliseconds)
    {
        reactionTimes.Add(milliseconds);
        Debug.Log($"Added reaction time: {milliseconds}ms. Total: {reactionTimes.Count}, Average: {AverageReactionTime}ms");
    }

    public void ResetReactionTimes()
    {
        reactionTimes.Clear();
        Debug.Log("Reaction times reset");
    }

    public void SaveAverageReactionTime()
    {
        if (reactionTimes.Count > 0)
        {
            float avgTime = AverageReactionTime;
            PlayerPrefs.SetFloat(LAST_REACTION_TIME_KEY, avgTime);
            PlayerPrefs.SetInt(HAS_REACTION_TIME_DATA_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log($"Saved average reaction time to PlayerPrefs: {avgTime}ms");
        }
    }

    public static float GetLastAverageReactionTime()
    {
        return PlayerPrefs.GetFloat(LAST_REACTION_TIME_KEY, 0f);
    }

    public static bool HasReactionTimeData()
    {
        return PlayerPrefs.GetInt(HAS_REACTION_TIME_DATA_KEY, 0) == 1;
    }
}
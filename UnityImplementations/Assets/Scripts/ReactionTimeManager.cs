using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionTimeManager : MonoBehaviour
{
    // Singleton instance
    public static ReactionTimeManager Instance { get; private set; }

    // List to store reaction times in milliseconds
    private List<int> reactionTimes = new List<int>();

    // Property to get the average reaction time
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

    // Property to get all reaction times
    public List<int> AllReactionTimes => new List<int>(reactionTimes);

    // Property to get the number of circles tapped
    public int TotalTaps => reactionTimes.Count;

    // PlayerPrefs key for saving average reaction time - made public for use in other scripts
    public const string LAST_REACTION_TIME_KEY = "LastAverageReactionTime";
    public const string HAS_REACTION_TIME_DATA_KEY = "HasReactionTimeData";

    private void Awake()
    {
        // Singleton pattern implementation
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

    // Add a new reaction time to the list
    public void AddReactionTime(int milliseconds)
    {
        reactionTimes.Add(milliseconds);
        Debug.Log($"Added reaction time: {milliseconds}ms. Total: {reactionTimes.Count}, Average: {AverageReactionTime}ms");
    }

    // Reset all stored reaction times
    public void ResetReactionTimes()
    {
        reactionTimes.Clear();
        Debug.Log("Reaction times reset");
    }

    // Save the last average reaction time to PlayerPrefs
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

    // Get the last saved average reaction time from PlayerPrefs
    public static float GetLastAverageReactionTime()
    {
        return PlayerPrefs.GetFloat(LAST_REACTION_TIME_KEY, 0f);
    }

    // Check if user has any reaction time data saved
    public static bool HasReactionTimeData()
    {
        return PlayerPrefs.GetInt(HAS_REACTION_TIME_DATA_KEY, 0) == 1;
    }
}
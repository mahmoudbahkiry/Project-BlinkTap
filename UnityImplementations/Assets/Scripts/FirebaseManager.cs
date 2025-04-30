using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    [SerializeField] private string backendUrl = "http://localhost:3000";

    private static FirebaseManager _instance;
    public static FirebaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FirebaseManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("FirebaseManager");
                    _instance = go.AddComponent<FirebaseManager>();
                    DontDestroyOnLoad(go);

                    if (PlayerPrefs.HasKey("UserEmail"))
                    {
                        string email = PlayerPrefs.GetString("UserEmail");
                        _instance.SetUserEmail(email);
                    }
                }
            }
            return _instance;
        }
    }

    private string userEmail = "";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetUserEmail(string email)
    {
        userEmail = email;
    }

    public string GetUserEmail()
    {
        if (string.IsNullOrEmpty(userEmail) && PlayerPrefs.HasKey("UserEmail"))
        {
            userEmail = PlayerPrefs.GetString("UserEmail");
        }

        return userEmail;
    }

    public void UploadTestScore(float averageReactionTime, Action<bool> callback = null)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(UploadScoreCoroutine(averageReactionTime, callback));
    }

    private IEnumerator UploadScoreCoroutine(float averageReactionTime, Action<bool> callback)
    {
        string url = $"{backendUrl}/scores";

        ScoreData scoreData = new ScoreData
        {
            email = userEmail,
            averageReactionTime = averageReactionTime,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string jsonData = JsonUtility.ToJson(scoreData);

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.POST, jsonData, response =>
        {
            bool success = response.IsSuccess;
            callback?.Invoke(success);
        }));
    }

    public void GetBestScore(Action<int> callback)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            callback?.Invoke(0);
            return;
        }

        StartCoroutine(GetBestScoreCoroutine(callback));
    }

    private IEnumerator GetBestScoreCoroutine(Action<int> callback)
    {
        string url = $"{backendUrl}/best-score?email={RESTClient.EscapeURL(userEmail)}";

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            int bestScore = 0;

            if (response.IsSuccess)
            {
                string responseText = response.Text;

                try
                {
                    BestScoreResponse scoreResponse = JsonUtility.FromJson<BestScoreResponse>(responseText);
                    if (scoreResponse != null && scoreResponse.bestScore > 0)
                    {
                        bestScore = scoreResponse.bestScore;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"FirebaseManager: Error parsing best score response: {e.Message}");
                }
            }

            callback?.Invoke(bestScore);
        }));
    }

    public void GetMostRecentReactionTime(Action<float> callback)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            callback?.Invoke(0);
            return;
        }

        StartCoroutine(GetMostRecentReactionTimeCoroutine(callback));
    }

    private IEnumerator GetMostRecentReactionTimeCoroutine(Action<float> callback)
    {
        string url = $"{backendUrl}/most-recent-score?email={RESTClient.EscapeURL(userEmail)}";

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            float recentTime = 0;

            if (response.IsSuccess)
            {
                string responseText = response.Text;

                try
                {
                    RecentScoreResponse scoreResponse = JsonUtility.FromJson<RecentScoreResponse>(responseText);
                    if (scoreResponse != null && scoreResponse.recentScore > 0)
                    {
                        recentTime = scoreResponse.recentScore;

                        PlayerPrefs.SetFloat(ReactionTimeManager.LAST_REACTION_TIME_KEY, recentTime);
                        PlayerPrefs.SetInt(ReactionTimeManager.HAS_REACTION_TIME_DATA_KEY, 1);
                        PlayerPrefs.Save();
                    }
                }
                catch (Exception e)
                {
                }
            }

            callback?.Invoke(recentTime);
        }));
    }

    public void CheckServerConnection(Action<bool> callback = null)
    {
        StartCoroutine(CheckServerConnectionCoroutine(callback));
    }

    private IEnumerator CheckServerConnectionCoroutine(Action<bool> callback)
    {
        string url = $"{backendUrl}/debug";

        yield return StartCoroutine(RESTClient.SendRequest(url, RESTClient.RequestType.GET, null, response =>
        {
            bool isConnected = response.IsSuccess;
            callback?.Invoke(isConnected);
        }));
    }

    public string GetBackendUrl()
    {
        return backendUrl;
    }

    public void SetBackendUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            backendUrl = url;
        }
    }

    [Serializable]
    private class ScoreData
    {
        public string email;
        public float averageReactionTime;
        public string timestamp;
    }

    [Serializable]
    private class BestScoreResponse
    {
        public int bestScore;
    }

    [Serializable]
    private class RecentScoreResponse
    {
        public float recentScore;
    }
}
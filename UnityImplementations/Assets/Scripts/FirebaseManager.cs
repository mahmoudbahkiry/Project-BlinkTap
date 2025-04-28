using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool success = false;

            if (request.result == UnityWebRequest.Result.Success)
            {
                success = true;
            }

            callback?.Invoke(success);
        }
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
        string url = $"{backendUrl}/best-score?email={UnityWebRequest.EscapeURL(userEmail)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            int bestScore = 0;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                try
                {
                    BestScoreResponse response = JsonUtility.FromJson<BestScoreResponse>(responseText);
                    if (response != null && response.bestScore > 0)
                    {
                        bestScore = response.bestScore;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"FirebaseManager: Error parsing best score response: {e.Message}");
                }
            }

            callback?.Invoke(bestScore);
        }
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
        string url = $"{backendUrl}/most-recent-score?email={UnityWebRequest.EscapeURL(userEmail)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;

            yield return request.SendWebRequest();

            float recentTime = 0;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                try
                {
                    RecentScoreResponse response = JsonUtility.FromJson<RecentScoreResponse>(responseText);
                    if (response != null && response.recentScore > 0)
                    {
                        recentTime = response.recentScore;

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
        }
    }

    public void CheckServerConnection(Action<bool> callback = null)
    {
        StartCoroutine(CheckServerConnectionCoroutine(callback));
    }

    private IEnumerator CheckServerConnectionCoroutine(Action<bool> callback)
    {
        string url = $"{backendUrl}/debug";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            bool isConnected = request.result == UnityWebRequest.Result.Success;

            callback?.Invoke(isConnected);
        }
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
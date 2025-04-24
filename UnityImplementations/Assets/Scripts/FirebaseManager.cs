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
                // Try to find an existing instance
                _instance = FindObjectOfType<FirebaseManager>();

                // If still null, create a new instance
                if (_instance == null)
                {
                    GameObject go = new GameObject("FirebaseManager");
                    _instance = go.AddComponent<FirebaseManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("FirebaseManager: Created new instance automatically");

                    // Try to get email from PlayerPrefs
                    if (PlayerPrefs.HasKey("UserEmail"))
                    {
                        string email = PlayerPrefs.GetString("UserEmail");
                        _instance.SetUserEmail(email);
                        Debug.Log($"FirebaseManager: Set email to: {email} from PlayerPrefs");
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
            Debug.Log("FirebaseManager: Instance set in Awake");
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            Debug.Log("FirebaseManager: Destroyed duplicate instance");
        }
    }

    public void SetUserEmail(string email)
    {
        userEmail = email;
        Debug.Log($"FirebaseManager: User email set to {email}");
    }

    public string GetUserEmail()
    {
        // If email is not set but exists in PlayerPrefs, use that
        if (string.IsNullOrEmpty(userEmail) && PlayerPrefs.HasKey("UserEmail"))
        {
            userEmail = PlayerPrefs.GetString("UserEmail");
            Debug.Log($"FirebaseManager: Retrieved email from PlayerPrefs: {userEmail}");
        }

        return userEmail;
    }

    public void UploadTestScore(float averageReactionTime, Action<bool> callback = null)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("FirebaseManager: Cannot upload score - user email not set!");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(UploadScoreCoroutine(averageReactionTime, callback));
    }

    private IEnumerator UploadScoreCoroutine(float averageReactionTime, Action<bool> callback)
    {
        string url = $"{backendUrl}/scores";

        // Create the score data
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

            Debug.Log($"FirebaseManager: Uploading score: {jsonData}");

            yield return request.SendWebRequest();

            bool success = false;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"FirebaseManager: Score upload successful: {request.downloadHandler.text}");
                success = true;
            }
            else
            {
                Debug.LogError($"FirebaseManager: Score upload failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }

            callback?.Invoke(success);
        }
    }

    public void GetBestScore(Action<int> callback)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("FirebaseManager: Cannot get best score - user email not set!");
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
            Debug.Log($"FirebaseManager: Fetching best score for {userEmail}");

            yield return request.SendWebRequest();

            int bestScore = 0;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"FirebaseManager: Best score response: {responseText}");

                try
                {
                    BestScoreResponse response = JsonUtility.FromJson<BestScoreResponse>(responseText);
                    if (response != null && response.bestScore > 0)
                    {
                        bestScore = response.bestScore;
                    }
                    else
                    {
                        Debug.Log("FirebaseManager: No best score available yet");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"FirebaseManager: Error parsing best score response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"FirebaseManager: Failed to get best score: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }

            callback?.Invoke(bestScore);
        }
    }

    public void GetMostRecentReactionTime(Action<float> callback)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("FirebaseManager: Cannot get recent reaction time - user email not set!");
            callback?.Invoke(0);
            return;
        }

        StartCoroutine(GetMostRecentReactionTimeCoroutine(callback));
    }

    private IEnumerator GetMostRecentReactionTimeCoroutine(Action<float> callback)
    {
        string url = $"{backendUrl}/most-recent-score?email={UnityWebRequest.EscapeURL(userEmail)}";
        Debug.Log($"FirebaseManager: Attempting to connect to: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            Debug.Log($"FirebaseManager: Fetching most recent reaction time for {userEmail}");

            // Set timeout to 10 seconds to avoid long waits if server is down
            request.timeout = 10;

            yield return request.SendWebRequest();

            float recentTime = 0;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"FirebaseManager: Most recent reaction time response: {responseText}");

                try
                {
                    RecentScoreResponse response = JsonUtility.FromJson<RecentScoreResponse>(responseText);
                    if (response != null && response.recentScore > 0)
                    {
                        recentTime = response.recentScore;

                        // Also save to PlayerPrefs for offline access
                        PlayerPrefs.SetFloat(ReactionTimeManager.LAST_REACTION_TIME_KEY, recentTime);
                        PlayerPrefs.SetInt(ReactionTimeManager.HAS_REACTION_TIME_DATA_KEY, 1);
                        PlayerPrefs.Save();

                        Debug.Log($"FirebaseManager: Most recent reaction time: {recentTime}ms (saved to PlayerPrefs)");
                    }
                    else
                    {
                        Debug.LogWarning($"FirebaseManager: No recent scores available yet for user {userEmail} or recentScore was 0");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"FirebaseManager: Error parsing recent score response: {e.Message}");
                    Debug.LogError($"Response received: '{responseText}'");
                    Debug.LogError($"Exception details: {e}");
                }
            }
            else
            {
                Debug.LogError($"FirebaseManager: Failed to get recent reaction time: {request.error}");
                Debug.LogError($"Response code: {request.responseCode}");
                Debug.LogError($"Is server running at {backendUrl}? Connection error: {request.error}");

                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.LogError("No response body received");
                }
            }

            callback?.Invoke(recentTime);
        }
    }

    // Method to check server connection and log result
    public void CheckServerConnection(Action<bool> callback = null)
    {
        StartCoroutine(CheckServerConnectionCoroutine(callback));
    }

    private IEnumerator CheckServerConnectionCoroutine(Action<bool> callback)
    {
        string url = $"{backendUrl}/debug";
        Debug.Log($"FirebaseManager: Checking server connection to {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            bool isConnected = request.result == UnityWebRequest.Result.Success;

            if (isConnected)
            {
                Debug.Log($"FirebaseManager: Successfully connected to server at {backendUrl}");
            }
            else
            {
                Debug.LogError($"FirebaseManager: Failed to connect to server at {backendUrl}. Error: {request.error}");
            }

            callback?.Invoke(isConnected);
        }
    }

    // Getter for backend URL for debugging
    public string GetBackendUrl()
    {
        return backendUrl;
    }

    // Method to set the backend URL (useful for testing different environments)
    public void SetBackendUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Debug.Log($"FirebaseManager: Setting backend URL from {backendUrl} to {url}");
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
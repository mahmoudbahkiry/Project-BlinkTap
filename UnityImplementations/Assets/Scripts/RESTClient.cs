using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class RESTClient
{
    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class Response
    {
        public bool IsSuccess { get; private set; }
        public string Text { get; private set; }
        public long StatusCode { get; private set; }
        public string Error { get; private set; }

        public Response(bool isSuccess, string text, long statusCode, string error = null)
        {
            IsSuccess = isSuccess;
            Text = text;
            StatusCode = statusCode;
            Error = error;
        }
    }

    public static IEnumerator SendRequest(string url, RequestType requestType, string jsonData = null, Action<Response> callback = null)
    {
        using (UnityWebRequest request = CreateRequest(url, requestType, jsonData))
        {
            yield return request.SendWebRequest();

            bool isSuccess = request.result == UnityWebRequest.Result.Success;
            string text = request.downloadHandler?.text ?? string.Empty;
            long statusCode = request.responseCode;
            string error = request.error;

            Response response = new Response(isSuccess, text, statusCode, error);
            callback?.Invoke(response);
        }
    }

    private static UnityWebRequest CreateRequest(string url, RequestType requestType, string jsonData = null)
    {
        UnityWebRequest request = null;

        switch (requestType)
        {
            case RequestType.GET:
                request = UnityWebRequest.Get(url);
                break;

            case RequestType.POST:
                request = new UnityWebRequest(url, "POST");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                break;

            case RequestType.PUT:
                request = new UnityWebRequest(url, "PUT");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                break;

            case RequestType.DELETE:
                request = new UnityWebRequest(url, "DELETE");
                request.downloadHandler = new DownloadHandlerBuffer();
                break;
        }

        return request;
    }

    public static string EscapeURL(string url)
    {
        return UnityWebRequest.EscapeURL(url);
    }
}
using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TwitterShare : MonoBehaviour
{
    public static TwitterShare Instance { get; private set; }

    [SerializeField] private string backendUrl = "https://imaikzz.pythonanywhere.com/";

    private string state;
    private HttpListener listener;

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("TwitterShare already exists, destroying new one");
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
        StartListener();
    }

    // --- 1. Begin login flow ---
    public void LoginToTwitter()
    {
        Application.OpenURL($"{backendUrl}/auth");
    }

    // --- 2. Start local HTTP listener to capture /callback?state=xxx ---
    private void StartListener()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:7890/callback/");
        try
        {
            listener.Start();
            Debug.Log("Started listener on http://localhost:7890/callback/");
            ListenAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Listener failed to start: " + e.Message);
        }
    }
    private async void ListenAsync()
    {
        while (true)
        {
            var context = await listener.GetContextAsync();
            var query = context.Request.QueryString;
            state = query.Get("state");

            // Respond to browser
            string html = "<html><body><h2>Login complete. You can return to the app.</h2></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            Debug.Log("OAuth callback received. State = " + state);

            // Now exchange the code with backend
            await ExchangeTokenWithBackend(state);
        }
    }

    // --- 3. Exchange state/code with backend to complete login ---
    private async Task ExchangeTokenWithBackend(string oauthState)
    {
        string json = JsonUtility.ToJson(new StateRequest { state = oauthState });

        using UnityWebRequest request = new UnityWebRequest($"{backendUrl}/token", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Token exchange failed: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Token exchange success: " + request.downloadHandler.text);
        }
    }

    // --- 4. Tweet after login ---
    public void ShareToTwitter(string message)
    {
        if (string.IsNullOrEmpty(state))
        {
            Debug.LogError("Cannot tweet — not logged in yet.");
            LoginToTwitter();
            return;
        }

        StartCoroutine(SendTweetCoroutine(message));
    }

    private IEnumerator SendTweetCoroutine(string message)
    {
        var tweetData = new TweetData { message = message, state = state };
        string json = JsonUtility.ToJson(tweetData);

        using UnityWebRequest request = new UnityWebRequest($"{backendUrl}/tweet", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("Tweet failed: " + request.downloadHandler.text);
        else
            Debug.Log("Tweet posted: " + request.downloadHandler.text);
    }

    private void OnApplicationQuit()
    {
        if (listener != null && listener.IsListening)
            listener.Stop();
    }

    [Serializable]
    private class TweetData
    {
        public string message;
        public string state;
    }

    [Serializable]
    private class StateRequest
    {
        public string state;
    }
}

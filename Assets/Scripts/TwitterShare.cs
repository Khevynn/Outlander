using System;
using UnityEngine;
using System.Web;

public class TwitterShare : MonoBehaviour
{
    public static TwitterShare Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("TwitterShare already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void ShareToTwitter(string message)
    {
        string tweet = HttpUtility.UrlEncode(message);
        string twitterUrl = $"https://twitter.com/intent/tweet?text={tweet}";
        Application.OpenURL(twitterUrl);
    }
}
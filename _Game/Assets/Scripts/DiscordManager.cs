using System;
using UnityEngine;
using Discord;

public class DiscordManager : MonoBehaviour
{
    public static DiscordManager Instance { get; private set; }
    
    [Header("Discord Connection")]
    [SerializeField] private long clientId = 123456789012345678; // Replace with your real client ID

    private Discord.Discord discord;
    private ActivityManager activityManager;
    private Activity currentActivity;

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("DiscordManager already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        discord = new Discord.Discord(clientId, (ulong)CreateFlags.Default); // Fixed here
        activityManager = discord.GetActivityManager();

        currentActivity = new Activity
        {
            State = "Traveling the universe",
            Details = "Playing Outlander",
            Timestamps =
            {
                Start = DateTimeOffset.Now.ToUnixTimeSeconds()
            },
            Assets =
            {
                LargeImage = "outlander", // Must match an image name uploaded in the Discord Developer Portal
                LargeText = "Outlander"
            }
        };

        activityManager.UpdateActivity(currentActivity, result =>
        {
            if (result == Result.Ok)
                Debug.Log("Rich Presence successfully set!");
            else
                Debug.LogError("Failed to set Rich Presence: " + result);
        });
    }
    private void Update()
    {
        discord.RunCallbacks();
    }
    
    public void UpdateActivityState(string state, string details)
    {
        currentActivity.State = state;
        currentActivity.Details = details;
        
        activityManager.UpdateActivity(currentActivity, result =>
        {
            if (result == Result.Ok)
                Debug.Log("Rich Presence successfully set!");
            else
                Debug.LogError("Failed to set Rich Presence: " + result);
        });
    }

    private void OnApplicationQuit()
    {
        discord.Dispose();
    }
}
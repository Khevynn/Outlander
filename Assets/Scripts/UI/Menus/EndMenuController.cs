using System;
using UnityEngine;

public class EndMenuController : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneLoader.Instance.LoadScene("StartMenu");
    }
    
    public void OnTweetButtonClicked()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        TwitterShare.Instance.ShareToTwitter($"I just finished Outlander! What an intense journey. {timestamp}\n #OutlanderGame #GameCompleted");
    }
}

using UnityEngine;

public class EndMenuController : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneLoader.Instance.LoadScene("StartMenu");
    }
    
    public void OnTweetButtonClicked()
    {
        TwitterShare.Instance.ShareToTwitter("I just finished Outlander! What an intense journey.\n #OutlanderGame #GameCompleted");
    }
}

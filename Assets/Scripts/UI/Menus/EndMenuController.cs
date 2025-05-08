using UnityEngine;

public class EndMenuController : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneLoader.Instance.LoadScene("StartMenu");
    }
}

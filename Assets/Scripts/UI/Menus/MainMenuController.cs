using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Control")] 
    [SerializeField] private string mainSceneName;

    public void Play()
    {
        SceneLoader.Instance.LoadScene(mainSceneName);
    }

    public void ConfirmQuit()
    {
        Application.Quit();
    }
}

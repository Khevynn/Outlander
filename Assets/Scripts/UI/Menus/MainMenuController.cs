using System;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    
    [Header("Scene Control")] 
    [SerializeField] private string mainSceneName;
    
    [Header("Discord Control")] 
    [SerializeField] private string newState;
    [SerializeField] private string newDetails;

    public void Play()
    {
        DiscordManager.Instance.UpdateActivityState(newState, newDetails);
        SceneLoader.Instance.LoadScene(mainSceneName);
    }

    public void ConfirmQuit()
    {
        Application.Quit();
    }
}

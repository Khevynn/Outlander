using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private VideoSettings currentVideoSettings;

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("GameManager already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
        
        SaveManager.LoadSave();
        if (SaveManager.savedGame == null)
        {
            SaveManager.NewSave();
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(this);
        currentVideoSettings = SaveManager.savedGame.GetSavedVideoSettings();
    }

    public void SaveVideoSettings(VideoSettings settings)
    {
        currentVideoSettings = settings;
        SaveManager.savedGame.SetSavedVideoSettings(settings);
        SaveManager.OverwriteSave();
    }
    
    public VideoSettings GetCurrentVideoSettings() => currentVideoSettings;
}

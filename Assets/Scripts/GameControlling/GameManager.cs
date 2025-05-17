using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private VideoSettings currentVideoSettings;
    [SerializeField] private List<AudioSource> gameSounds;

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
        GetAllGameSounds();
    }

    public void GetAllGameSounds()
    {
        gameSounds.Clear();
        var audioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < audioSources.Length; ++i)
        {
            if (audioSources[i].gameObject.layer != 5)
            {
                gameSounds.Add(audioSources[i]);
            }
        }
    }

    public void Win()
    {
        Cursor.lockState = CursorLockMode.None;
        SceneLoader.Instance.LoadScene("WinMenu");
    }
    public void Lose()
    {
        Cursor.lockState = CursorLockMode.None;
        SceneLoader.Instance.LoadScene("LostMenu");
    }
    public void BackToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        SceneLoader.Instance.LoadScene("StartMenu");
    }

    public void PauseAllGameSounds()
    {
        for (int i = 0; i < gameSounds.Count; ++i)
        {
            if (!gameSounds[i])
            {
                GetAllGameSounds();
            }
            gameSounds[i].Pause();
        }
    }
    public void UnpauseAllGameSounds()
    {
        for (int i = 0; i < gameSounds.Count; ++i)
        {
            if (!gameSounds[i])
            {
                GetAllGameSounds();
            }
            gameSounds[i].UnPause();
        }
    }
    
    public void SaveVideoSettings(VideoSettings settings)
    {
        currentVideoSettings = settings;
        SaveManager.savedGame.SetSavedVideoSettings(settings);
        SaveManager.OverwriteSave();
    }
    
    public VideoSettings GetCurrentVideoSettings() => currentVideoSettings;
}

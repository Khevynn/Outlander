using UnityEngine;

public class GameSettingsController : MonoBehaviour
{
    public static GameSettingsController Instance { get; private set; }
    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("GameSettingsController already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }
    
    private void Start()
    {
        DontDestroyOnLoad(this);
    }
    
    public void ApplyVideoSettings(VideoSettings settings)
    {
        Screen.SetResolution(settings.ScreenWidth, settings.ScreenHeight, settings.isFullScreen);
        QualitySettings.vSyncCount = settings.VsyncCount;
        Application.targetFrameRate = settings.FPSCap;
        QualitySettings.SetQualityLevel(settings.QualityLevel, true);
        
        GameManager.Instance.SaveVideoSettings(settings);
    }
    public void ApplyAudioSettings()
    {
        
    }
}

[System.Serializable]
public class VideoSettings
{
    public int ScreenWidth;
    public int ScreenHeight;
    public bool isFullScreen;
    
    public int FPSCap;
    public int VsyncCount;

    public int QualityLevel;

    public VideoSettings(Resolution screenResolution, bool fullscreen, int fpsCap, int vsyncCount, int qualityLevel)
    {
        ScreenWidth = screenResolution.width;
        ScreenHeight = screenResolution.height;
        isFullScreen = fullscreen;
        FPSCap = fpsCap;
        VsyncCount = vsyncCount;
        QualityLevel = qualityLevel;
    }
}
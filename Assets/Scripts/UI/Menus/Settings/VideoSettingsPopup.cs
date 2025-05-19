using System;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

public class VideoSettingsPopup : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionsDropdown;
    [SerializeField] private TMP_Dropdown fpsDropdown;
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    private List<Resolution> filteredResolutions;
    private List<FullScreenMode> screenModes;

    private void Start()
    {
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        SetupFPSDropdown();
        SetupGraphicsDropdown();
        
        if (PlayerPrefs.HasKey("ScreenMode"))
        {
            LoadVideoSettings();
        }
        
        CallApplySettings();
    }

    private void LoadVideoSettings()
    {
        windowModeDropdown.value = PlayerPrefs.GetInt("ScreenMode"); 
        resolutionsDropdown.value = PlayerPrefs.GetInt("SelectedResolution"); 
        graphicsDropdown.value = PlayerPrefs.GetInt("QualityLevel"); 
        fpsDropdown.value = PlayerPrefs.GetInt("FpsCap"); 
    }
    public void CallApplySettings()
    {
        var screenMode = screenModes[windowModeDropdown.value];
        var selectedResolution = filteredResolutions[resolutionsDropdown.value];
        var qualityLevel = graphicsDropdown.value;
        
        int fpsCap;
        switch (fpsDropdown.value)
        {
            case 0:
                fpsCap = -1;
                break;
            case 1:
                fpsCap = 120;
                break;
            case 2:
                fpsCap = 60;
                break;
            default:
                fpsCap = 30;
                break;
        }

        PlayerPrefs.SetInt("ScreenMode", windowModeDropdown.value); 
        PlayerPrefs.SetInt("SelectedResolution", resolutionsDropdown.value); 
        PlayerPrefs.SetInt("QualityLevel", graphicsDropdown.value); 
        PlayerPrefs.SetInt("FpsCap", fpsDropdown.value); 
        
        var settings = new VideoSettings(selectedResolution, screenMode == FullScreenMode.FullScreenWindow, fpsCap, 0, qualityLevel);
        ApplyVideoSettings(settings);
    }
    
    #region Resolution Dropdown Setup
    
    private void SetupResolutionDropdown()
    {
        // Ensure to retain this region for the resolution functionality
        var resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        FilterResolutions(resolutions);

        // Update the options of screen size
        var options = new List<string>();
        AddResolutionsToList(options);
        SetResolutionDropdownOptions(options);
    }
    private void FilterResolutions(Resolution[] resolutions)
    {
        // Filter Resolutions According to the current refresh rate of the monitor
        var currentRefreshRateRatio = (float)Screen.currentResolution.refreshRateRatio.value;
        foreach (Resolution resolution in resolutions)
        {
            if (Mathf.Approximately((float)resolution.refreshRateRatio.value, currentRefreshRateRatio))
            {
                filteredResolutions.Add(resolution);
            }
        }

        // Check if filteredResolutions is empty, fallback to default resolutions if needed
        if (filteredResolutions.Count == 0)
        {
            // Optionally log a warning if no resolutions match the refresh rate ratio
            Debug.LogWarning("No resolutions found matching the current refresh rate ratio. Using all available resolutions.");
            filteredResolutions.AddRange(resolutions);
        }
    }
    private void AddResolutionsToList(List<string> options)
    {
        for (int i = 0; i < filteredResolutions.Count; ++i)
        {
            var resolution = filteredResolutions[i];
            var resolutionOptionText= $"{resolution.width}x{resolution.height}";
            options.Add(resolutionOptionText);
        }
    }
    private void SetResolutionDropdownOptions(List<string> options)
    {
        filteredResolutions.Reverse();
        options.Reverse();
        
        resolutionsDropdown.ClearOptions();
        resolutionsDropdown.AddOptions(options);
        resolutionsDropdown.RefreshShownValue();
    }

    #endregion
    private void SetupScreenModeDropdown()
    {
        screenModes = new List<FullScreenMode>();
        screenModes.Add(FullScreenMode.FullScreenWindow);
        screenModes.Add(FullScreenMode.Windowed);

        var screenModeOptions = new List<String>();
        screenModeOptions.Add("Fullscreen");
        screenModeOptions.Add("Windowed");
        
        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(screenModeOptions);
        windowModeDropdown.RefreshShownValue();
    }
    private void SetupFPSDropdown()
    {
        //Fps limit dropdown Update
        var fpsOptions = new List<string>();
        fpsOptions.Add("No Limit");
        fpsOptions.Add("120");
        fpsOptions.Add("60");
        fpsOptions.Add("30");
        
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(fpsOptions);
        fpsDropdown.RefreshShownValue();
    }
    private void SetupGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(QualitySettings.names.ToList());
        graphicsDropdown.RefreshShownValue();
    }
    
    private void ApplyVideoSettings(VideoSettings settings)
    {
        Screen.SetResolution(settings.ScreenWidth, settings.ScreenHeight, settings.isFullScreen);
        QualitySettings.vSyncCount = settings.VsyncCount;
        Application.targetFrameRate = settings.FPSCap;
        QualitySettings.SetQualityLevel(settings.QualityLevel, true);
    }
}

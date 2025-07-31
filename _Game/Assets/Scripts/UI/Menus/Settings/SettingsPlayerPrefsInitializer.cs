using System;
using UnityEngine;

public class SettingsPlayerPrefsInitializer : MonoBehaviour
{
    [Header("Start Control")]
    [SerializeField] private GameObject videoSettingsObj;
    [SerializeField] private GameObject audioSettingsObj;
    [SerializeField] private GameObject controlsSettingsObj;
    
    [Header("Pause Menu Integration")]
    [SerializeField] private GameObject pauseMenuObj;
    [SerializeField] private GameObject mainPauseScreen;
    [SerializeField] private GameObject settingsPauseScreen;

    private void Start()
    {
        videoSettingsObj.SetActive(false);
        audioSettingsObj.SetActive(false);
        controlsSettingsObj.SetActive(false);
        if (pauseMenuObj)
        {
            pauseMenuObj.SetActive(false);
            mainPauseScreen.SetActive(true);
            settingsPauseScreen.SetActive(false);
        }
    }
}

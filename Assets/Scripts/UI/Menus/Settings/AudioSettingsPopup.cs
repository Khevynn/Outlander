using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsPopup : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider fxVolumeSlider;
    
    private void Start()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            LoadVolume();
        }
        
        SetMasterVolume();
        SetMusicVolume();
        SetSfxVolume();
    }

    public void SetMasterVolume()
    {
        var volume = masterVolumeSlider.value;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    public void SetMusicVolume()
    {
        var volume = musicVolumeSlider.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSfxVolume()
    {
        var volume = fxVolumeSlider.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    private void LoadVolume()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        fxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}

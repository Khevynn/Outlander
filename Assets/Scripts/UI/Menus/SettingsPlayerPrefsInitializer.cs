using UnityEngine;

public class SettingsPlayerPrefsInitializer : MonoBehaviour
{
    [Header("Start Control")]
    [SerializeField] private GameObject pauseMenuObj;
    [SerializeField] private GameObject videoSettingsObj;
    [SerializeField] private GameObject audioSettingsObj;
    
    private void Start()
    {
        videoSettingsObj.SetActive(false);
        audioSettingsObj.SetActive(false);
        
        if(pauseMenuObj)
            pauseMenuObj.SetActive(false);
    }
}

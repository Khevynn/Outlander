using UnityEngine;

[System.Serializable]
public class Save
{
    public static Save instance;

    private VideoSettings savedVideoSettings;

    public Save()
    {
        instance = this;
    }

    public void SetSavedVideoSettings(VideoSettings newSettings) => savedVideoSettings = newSettings;
    public VideoSettings GetSavedVideoSettings() => savedVideoSettings;
}

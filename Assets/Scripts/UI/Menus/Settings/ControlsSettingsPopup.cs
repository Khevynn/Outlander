using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsSettingsPopup : MonoBehaviour
{
    [Header("UI References")] 
    [SerializeField] private Slider sensitivitySlider;

    private PlayerInput _playerInput;

    private void Awake()
    {
        if (_playerInput == null)
        {
            Debug.LogError("PlayerInput component not found!");
        }
    }
    private void Start()
    {
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            ReloadSensitivity();
        }
    }
    private void OnEnable()
    {
        ReloadSensitivity();
    }

    public void ReloadSensitivity()
    {
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity");
        }
    }

    public void ApplySettings()
    {
        var controls = new Controls(sensitivitySlider.value);
        ApplyControls(controls);
    }
    private void ApplyControls(Controls controls)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", controls.Sensitivity);

        if (CamController.Instance != null)
        {
            CamController.Instance.SetMouseSensitivity();
        }
    }
}

[Serializable]
public class Controls
{
    public float Sensitivity;

    public Controls(float sensitivity)
    {
        Sensitivity = sensitivity;
    }
}

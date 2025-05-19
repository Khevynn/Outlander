using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensitivityController : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInputField;

    public void UpdateInputFieldValues()
    {
        sensitivityInputField.text = $"{Math.Round(sensitivitySlider.value, 3)}";
    }
    public void UpdateSliderValues()
    {
        float.TryParse(sensitivityInputField.text, out float value);
        value = Mathf.Min(value, 1);
        
        sensitivitySlider.value = value;
        UpdateInputFieldValues();
    }
}

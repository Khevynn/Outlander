using System;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    const float fpsMeasurePeriod = 0.5f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    const string display = "FPS: {0}";
    private TMP_Text m_Text;
    
    private void Start()
    {
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        m_Text = GetComponent<TMP_Text>();
        SaveManager.StartNewPerformanceGraph();
    }
    private void Update()
    {
        // measure average frames per second
        m_FpsAccumulator++;
        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = (int) (m_FpsAccumulator/fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
            var currentFps = string.Format(display, m_CurrentFps);
            
            m_Text.text = currentFps;
            SaveManager.WritePerformance(currentFps);
        }
    }
}

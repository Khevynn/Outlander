using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UICollectingResources : MonoBehaviour
{
    public static UICollectingResources Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject collectingResourcesUI;
    [SerializeField] private Slider collectingResourcesSlider;
    
    private float _maxHoldTimeToDrop = 0f;    
    private float _currentHoldTimeToDrop = 0f;    
    
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("UICollectingResources already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }

    public void SetIsCollecting(bool inIsCollecting)
    { 
        collectingResourcesUI.SetActive(inIsCollecting);
    } 
    public void SetMaxHoldTimeToDrop(float inMaxHoldTimeToDrop) => _maxHoldTimeToDrop = inMaxHoldTimeToDrop;
    public void SetCurrentHoldTimeToDrop(float inCurrentHoldTimeToDrop)
    { 
        _currentHoldTimeToDrop = inCurrentHoldTimeToDrop;

        float value = _currentHoldTimeToDrop / _maxHoldTimeToDrop;
        collectingResourcesSlider.value = value;
    }
}

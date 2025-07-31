using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectableResource : MonoBehaviour, IInteract
{
    [Header("Resources Control")]
    public static CollectableResource CurrentCollecting;
    
    [Header("Collecting Control")]
    [SerializeField] private float maxHoldTimeToDrop;
    [SerializeField] private AudioSource audioSource;
    private float _currentHoldTimeToDrop;
    private bool _isCollecting;
    
    private InputAction _interactAction;
    private DropItemsComponent _dropitemsComponent;
    
    private void Start()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
        _dropitemsComponent = GetComponent<DropItemsComponent>();
    }
    private void FixedUpdate()
    {
        if (CurrentCollecting != this)
            return;
        
        if (!_interactAction.IsPressed() && UICollectingResources.Instance)
        {
            _isCollecting = false;
            UICollectingResources.Instance.SetIsCollecting(false);
            return;
        }
        
        if(_isCollecting && _currentHoldTimeToDrop < maxHoldTimeToDrop)
        {
            _currentHoldTimeToDrop += Time.fixedDeltaTime;
        }
        
        if(_currentHoldTimeToDrop >= maxHoldTimeToDrop)
        {
            UICollectingResources.Instance.SetIsCollecting(false);
            _dropitemsComponent.DropItems();
            gameObject.SetActive(false);
        }
        
        UICollectingResources.Instance.SetCurrentHoldTimeToDrop(_currentHoldTimeToDrop);
    }

    public virtual void OnInteract()
    {
        _isCollecting = true;
        _currentHoldTimeToDrop = 0f;
        CurrentCollecting = this;
        StartUIForCollecting();
        audioSource.Play();
    }
    
    private void StartUIForCollecting()
    {
        UICollectingResources.Instance.SetIsCollecting(true);
        UICollectingResources.Instance.SetMaxHoldTimeToDrop(maxHoldTimeToDrop);
        UICollectingResources.Instance.SetCurrentHoldTimeToDrop(_currentHoldTimeToDrop);
    }
}

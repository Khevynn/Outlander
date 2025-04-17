using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct DroppableItem
{
    public Item itemPrefab;
    public int minAmount;
    public int maxAmount;
    public float chanceOfDropping;
}

public class CollectableResource : MonoBehaviour, IInteract
{
    [Header("Resources Control")]
    public static CollectableResource CurrentCollecting;
    [SerializeField] private List<DroppableItem> itemsToDrop;
    
    [Header("Collecting Control")]
    [SerializeField] private float maxHoldTimeToDrop;
    private float _currentHoldTimeToDrop;
    private bool _isCollecting;
    
    private InputAction _interactAction;
    
    private void Start()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
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
            DropItems();
        }
        
        UICollectingResources.Instance.SetCurrentHoldTimeToDrop(_currentHoldTimeToDrop);
    }

    public virtual void OnInteract()
    {
        _isCollecting = true;
        _currentHoldTimeToDrop = 0f;
        CurrentCollecting = this;
        StartUIForCollecting();
    }
    private void DropItems()
    {
        for(int i = 0; i < itemsToDrop.Count; i++)
        {
            if (UnityEngine.Random.Range(0f, 100f) < itemsToDrop[i].chanceOfDropping)
            {
                Item item = ItemsPool.Instance.GetItemWithId(itemsToDrop[i].itemPrefab.GetItemData().Id);
                item.SetAmountOfItems(UnityEngine.Random.Range(itemsToDrop[i].minAmount, itemsToDrop[i].maxAmount + 1));

                item.transform.position = transform.position + new Vector3(0f,.5f,0f);
                item.gameObject.SetActive(true);
            }
        }
        
        gameObject.SetActive(false);
    }
    
    private void StartUIForCollecting()
    {
        UICollectingResources.Instance.SetIsCollecting(true);
        UICollectingResources.Instance.SetMaxHoldTimeToDrop(maxHoldTimeToDrop);
        UICollectingResources.Instance.SetCurrentHoldTimeToDrop(_currentHoldTimeToDrop);
    }
}

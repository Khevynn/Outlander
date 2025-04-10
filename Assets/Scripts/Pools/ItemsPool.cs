using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsPool : MonoBehaviour
{
    public static ItemsPool Instance { get; private set; }
    
    [SerializeField] private List<GameObject> _itemsPrefabs;
    private List<Item> _availableItemsInPool = new List<Item>();
    private List<Item> _nonAvailableItemsInPool = new List<Item>();
    
    void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("ItemObjectsPool already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;

        SortPrefabsById();
    }

    private void Start()
    {
        SpawnInitialItems();
    }

    private void SpawnInitialItems()
    {
        for(int i = 0; i < _itemsPrefabs.Count; i++)
        {
            Item item = Instantiate(_itemsPrefabs[i], transform.position, Quaternion.identity, transform).GetComponent<Item>();
            _availableItemsInPool.Add(item);
        }
    }

    public Item GetItemWithId(int itemId)
    {
        Item itemToReturn = null;
        bool couldFindItem = false;
        for(int i = 0; i < _availableItemsInPool.Count; i++)
        {
            if(_availableItemsInPool[i].GetItemData().Id == itemId)
            {
                couldFindItem = true;
                itemToReturn = _availableItemsInPool[i];
            }
        }

        if (!couldFindItem)
        {
            itemToReturn = Instantiate(_itemsPrefabs[itemId], transform.position, Quaternion.identity, transform).GetComponent<Item>();
        }
        
        _availableItemsInPool.Remove(itemToReturn);
        _nonAvailableItemsInPool.Add(itemToReturn);
        return itemToReturn;
    }
    public void ReturnItemToPool(Item item)
    {
        _availableItemsInPool.Add(item);
        _nonAvailableItemsInPool.Remove(item);
    }
    
    private void SortPrefabsById()
    {
        _itemsPrefabs.Sort((GameObject a, GameObject b) => 
            a.GetComponent<Item>().GetItemData().Id.CompareTo(b.GetComponent<Item>().GetItemData().Id)
        );
    }
}

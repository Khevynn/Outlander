using System;
using System.Collections.Generic;
using UnityEngine;

public struct ItemInInventory
{
    private static ItemUI _uiReference;
    private InWorldItem _inWorldItemReference;
    private int _quantity;
    
    public ItemInInventory(InWorldItem inWorldItemReference, int quantity)
    {
        this._inWorldItemReference = inWorldItemReference;
        this._quantity = quantity;
    }
    
    public ItemUI UIReference { get => _uiReference; set => _uiReference = value; }
    public InWorldItem InWorldItemReference { get => _inWorldItemReference; }
    public int Quantity { get => _quantity; }
    
    
    public void AddQuantity(int amount) { _quantity += amount; UIReference.UpdateItemQuantity(Quantity); }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("UI Control")]
    [SerializeField] private GameObject inventoryUIParent;
    [SerializeField] private GameObject itemPrefab;
    
    [Header("Inventory Control")]
    [SerializeField] private int inventoryMaxSize = 10;
    
    public List<ItemInInventory> InventoryList = new List<ItemInInventory>();
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void AddItem(InWorldItem inWorldItem)
    {
        if (InventoryList.Count >= inventoryMaxSize)
            return;
        
        bool couldStack = false;
        
        for(int i = 0; i < InventoryList.Count; i++)
        {
            if (InventoryList[i].InWorldItemReference == inWorldItem && InventoryList[i].InWorldItemReference.IsStackable)
            {
                InventoryList[i].AddQuantity(1);
                couldStack = true;
                break;
            }
        }

        if (!couldStack)
        {
            var itemInInventory = new ItemInInventory(inWorldItem, 1);
            InventoryList.Add(itemInInventory);
            AddToInventoryUI(itemInInventory);
        }
    }

    private void AddToInventoryUI(ItemInInventory item)
    {
        var itemUI = Instantiate(itemPrefab, inventoryUIParent.transform);
        
        item.UIReference = itemUI.GetComponent<ItemUI>();
        item.UIReference.SetItem(item);
    }
}

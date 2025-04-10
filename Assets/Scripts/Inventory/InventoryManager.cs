using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private PlayerController _playerController;
    
    [Header("Inventory Controlling")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private GameObject[] itemStackPrefabs;
    [SerializeField] private GameObject slotsParent;
    private GameObject _dropArea;
    
    [Header("Popup Information")]
    [SerializeField] private GameObject popupGameObject;
    [SerializeField] private TMP_Text popupTitle;
    [SerializeField] private TMP_Text popupDescription;
    [SerializeField] private Image popupImage;
    
    [Header("Inventory")]
    private List<ItemSlot> _itemsInInventory = new List<ItemSlot>();
    private List<ItemSlot> _availableSlots = new List<ItemSlot>();
    
    void Awake()
    {
        if (Instance)
        {
            Debug.Log("InventoryManager already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }

    private void Start()
    {
        _dropArea = GameObject.FindWithTag("DropArea");
        _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        
        _dropArea.transform.parent.gameObject.SetActive(false);
        _dropArea.SetActive(false);
        
        CreateDefaultSlots();
    }

    public void CollectItem(Item item)
    {
        int remainingItems = item.GetAmountOfItems();
        for (int i = 0; i < _itemsInInventory.Count; i++)
        {
            if (_itemsInInventory[i].ItemData.Id == item.GetItemData().Id && _itemsInInventory[i].ItemData.IsStackable)
            {
                _itemsInInventory[i].AddItem(item.GetAmountOfItems(), out remainingItems);
                
                if(remainingItems <= 0)
                    break;
            }
        }
        
        if(remainingItems > 0)
            CreateNewSlots(item.GetItemData(), remainingItems);
    }
    public void DropItems(ItemSlot itemSlot)
    {
        Item itemToDrop = ItemsPool.Instance.GetItemWithId(itemSlot.ItemData.Id);
        itemToDrop.transform.position = _playerController.transform.position;
        itemToDrop.gameObject.SetActive(true);

        itemToDrop.SetItemData(itemSlot.ItemData);
        itemToDrop.SetAmountOfItems(itemSlot.Quantity);

        _itemsInInventory.Remove(itemSlot);
        ReturnSlotToPool(itemSlot);
    }

    public void ShowOrHideItemInfoPopup(ItemData itemData)
    {
        popupTitle.text = itemData.Name;
        popupDescription.text = itemData.Description;
        popupImage.sprite = itemData.Icon;
        
        popupImage.preserveAspect = true;
        
        popupGameObject.SetActive(!popupGameObject.activeSelf);
    }

    private void CreateDefaultSlots()
    {
        for (int i = 0; i < 10; i++)
        {
            var newSlot = Instantiate(itemSlotPrefab, slotsParent.transform).GetComponent<ItemSlot>();
            newSlot.gameObject.SetActive(false);
            _availableSlots.Add(newSlot);
        }
    }
    private void CreateNewSlots(ItemData itemData, int amountOfItems)
    {
        int itemsToAdd = amountOfItems;
        int nOfFullSlotsToAdd = amountOfItems / itemData.MaxStack;
        
        for (int i = 0; i < nOfFullSlotsToAdd; ++i)
        {
            var fullSlot = GetAnAvailableSlot();
            fullSlot.SetItem(itemData, itemData.MaxStack);
            
            _itemsInInventory.Add(fullSlot);
            itemsToAdd -= itemData.MaxStack;
        }

        if (itemsToAdd > 0)
        {
            var lastSlot = GetAnAvailableSlot();
            lastSlot.SetItem(itemData, itemsToAdd);
        
            _itemsInInventory.Add(lastSlot);
        }
    }

    public void ActivateDropArea()
    {
        _dropArea.SetActive(true);
    }
    public void DeactivateDropArea()
    {
        _dropArea.SetActive(false);
    }
    
    private ItemSlot GetAnAvailableSlot()
    {
        var slot = _availableSlots[0];
        if (slot == null)
        {
            slot = Instantiate(itemSlotPrefab, slotsParent.transform).GetComponent<ItemSlot>();
        }
        slot.gameObject.SetActive(true);
        _availableSlots.RemoveAt(0);
        return slot;
    }
    private void ReturnSlotToPool(ItemSlot slot)
    {
        slot.gameObject.SetActive(false);
        _availableSlots.Add(slot);
    }
}

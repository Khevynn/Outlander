using System;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Audio Settings")] 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip grabItemClip;
    [SerializeField] private AudioClip dropItemClip;
    [SerializeField] private AudioClip openBackpackClip;
    [SerializeField] private AudioClip craftItemClip;
    [SerializeField] private AudioClip failCraftClip;
    
    [Header("Inventory")]
    private List<StoreSlot> _itemsInInventory = new List<StoreSlot>();
    private List<StoreSlot> _availableSlots = new List<StoreSlot>();
    
    private void Awake()
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

    public void CollectItem(ItemData itemData, int quantity)
    {
        int remainingItems = quantity;
        for (int i = 0; i < _itemsInInventory.Count; ++i)
        {
            if (_itemsInInventory[i].ItemData.Id == itemData.Id && itemData.IsStackable)
            {
                _itemsInInventory[i].AddItem(quantity, out remainingItems);
                if (remainingItems <= 0)
                    break;
            }
        }

        if (remainingItems > 0)
            CreateNewSlots(itemData, remainingItems);
        
        PlayGrabItemSound();
    }
    public void CollectItem(Item item)
    {
        CollectItem(item.GetItemData(), item.GetAmountOfItems());
    }
    
    public void DropItems(StoreSlot storeSlot)
    {
        Item itemToDrop = ItemsPool.Instance.GetItemWithId(storeSlot.ItemData.Id);
        itemToDrop.transform.position = _playerController.transform.position;
        itemToDrop.gameObject.SetActive(true);

        itemToDrop.SetItemData(storeSlot.ItemData);
        itemToDrop.SetAmountOfItems(storeSlot.Quantity);

        _itemsInInventory.Remove(storeSlot);
        ReturnSlotToPool(storeSlot);
        
        PlayDropItemSound();
    }

    public CraftResult CraftItem(ItemRecipe itemRecipe)
    {
        var result = new CraftResult();
        bool hasAllItems = VerifyIfThePlayerHasAllItems(itemRecipe);
        if (!hasAllItems)
        {
            result.ResultType = CraftResultType.MissingItems;
            result.Message = "Missing Items!";
            PlayFailCraftSound();
            return result;
        }

        DecreaseItemsFromInventory(itemRecipe);
        
        CollectItem(itemRecipe.GetItemToCraft(), itemRecipe.GetAmountToCraft());
        CompactInventoryStacks();
        
        result.ResultType = CraftResultType.Success;
        result.Message = "Successfully Crafted!";
        PlayCraftItemSound();
        
        return result;
    }
    private bool VerifyIfThePlayerHasAllItems(ItemRecipe itemRecipe)
    {
        // Build a dictionary of available quantities by item ID
        Dictionary<int, int> inventoryItemCounts = new();

        foreach (var slot in _itemsInInventory)
        {
            int id = slot.ItemData.Id;
            if (!inventoryItemCounts.ContainsKey(id))
                inventoryItemCounts[id] = 0;

            inventoryItemCounts[id] += slot.Quantity;
        }

        // Check if we have enough of each required item
        foreach (var ingredient in itemRecipe.GetNecessaryItems())
        {
            int id = ingredient.itemData.Id;
            int requiredAmount = ingredient.quantity;

            if (!inventoryItemCounts.ContainsKey(id) || inventoryItemCounts[id] < requiredAmount)
            {
                return false;
            }
        }

        return true;
    }
    private void DecreaseItemsFromInventory(ItemRecipe itemRecipe)
    {
        foreach (var ingredient in itemRecipe.GetNecessaryItems())
        {
            int id = ingredient.itemData.Id;
            int remaining = ingredient.quantity;

            // Find all inventory slots that match the item
            foreach (var slot in _itemsInInventory)
            {
                if (slot.ItemData.Id != id)
                    continue;

                slot.DecreaseItem(remaining, out remaining);
                if (remaining <= 0)
                    break;
            }
        }
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
        for (int i = 0; i < 10; ++i)
        {
            var newSlot = Instantiate(itemSlotPrefab, slotsParent.transform).GetComponent<StoreSlot>();
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
    
    public void CompactInventoryStacks()
    {
        // Group all stackable items by their ID
        var stackableGroups = _itemsInInventory
            .Where(slot => slot.ItemData.IsStackable)
            .GroupBy(slot => slot.ItemData.Id);

        List<StoreSlot> newSlots = new List<StoreSlot>();

        foreach (var group in stackableGroups)
        {
            var itemData = group.First().ItemData;
            int totalQuantity = group.Sum(slot => slot.Quantity);

            // Remove the current slots
            foreach (var slot in group)
            {
                ReturnSlotToPool(slot);
            }
            _itemsInInventory.RemoveAll(slot => slot.ItemData.Id == itemData.Id);

            // Rebuild slots using total quantity and MaxStack
            while (totalQuantity > 0)
            {
                int stackAmount = Mathf.Min(totalQuantity, itemData.MaxStack);
                var newSlot = GetAnAvailableSlot();
                newSlot.SetItem(itemData, stackAmount);
                newSlots.Add(newSlot);
                totalQuantity -= stackAmount;
            }
        }

        // Add all new compacted slots back to inventory
        _itemsInInventory.AddRange(newSlots);
    }
    
    public StoreSlot SearchItemInInventory(int necessaryItemID)
    {
        for(int i = 0; i < _itemsInInventory.Count; ++i)
        {
            if (_itemsInInventory[i].ItemData.Id == necessaryItemID)
            {
                return _itemsInInventory[i];
            }
        }

        return null;
    }
    public StoreSlot GetAnAvailableSlot()
    {
        var slot = _availableSlots[0];
        if (slot == null)
        {
            slot = Instantiate(itemSlotPrefab, slotsParent.transform).GetComponent<StoreSlot>();
        }
        slot.gameObject.SetActive(true);
        _availableSlots.RemoveAt(0);
        return slot;
    }
    public void ReturnSlotToPool(StoreSlot slot)
    {
        slot.gameObject.SetActive(false);
        _availableSlots.Add(slot);
    }
    
    private void PlayGrabItemSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(grabItemClip);
    }
    private void PlayDropItemSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(dropItemClip);
    }
    public void PlayCraftItemSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(craftItemClip);
    }
    public void PlayFailCraftSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(failCraftClip);
    }
    
    public void PlayOpenBackpackSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(openBackpackClip);
    }
    public void PlayCloseBackpackSound()
    {
        audioSource.pitch = -1;
        audioSource.clip = openBackpackClip;
        audioSource.timeSamples = audioSource.clip.samples - 1;
        audioSource.Play();
    }
}

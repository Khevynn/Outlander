using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image itemSprite;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemQuantityText;
    
    public void SetItem(ItemInInventory itemInInventory)
    {
        itemSprite.sprite = itemInInventory.InWorldItemReference.Icon;
        itemNameText.text = itemInInventory.InWorldItemReference.Name;
        itemQuantityText.text = itemInInventory.Quantity.ToString();
    }
    
    public void UpdateItemQuantity(int newAmount)
    {
        itemQuantityText.text = newAmount.ToString();
    }
}

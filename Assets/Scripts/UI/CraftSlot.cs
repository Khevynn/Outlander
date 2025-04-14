using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftSlot : MonoBehaviour
{
    [SerializeField] private ItemRecipe itemRecipe;
    [SerializeField] private UICraftingItems craftingController;

    [Header("References")] 
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemQuantity;
    
    private void Start()
    {
        itemIcon.sprite = itemRecipe.GetItemToCraft().Icon;
        itemName.text = itemRecipe.GetItemToCraft().Name;
        itemQuantity.text = itemRecipe.GetAmountToCraft().ToString();
    }

    public void CallUIShowCraftPopup()
    {
        craftingController.UIShowCraftPopup(itemRecipe);
    }
}

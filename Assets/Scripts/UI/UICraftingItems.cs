using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CraftResultType { Success, MissingItems, Error }
public struct CraftResult
{
    public CraftResultType ResultType;
    public string Message;
}

public class UICraftingItems : MonoBehaviour
{
    private ItemRecipe _currentlySelectedRecipe;
    [Header("Craft UI")]
    [SerializeField] private GameObject craftPopup;
    [SerializeField] private Sprite noIngredientSprite;
    
    [Header("Item To Craft")]
    [SerializeField] private Image itemToCraftIcon;
    [SerializeField] private TMP_Text itemToCraftText;
    [SerializeField] private TMP_Text feedbackMessage;
    
    [Header("Ingredient 1")]
    [SerializeField] private Image item1Icon;
    [SerializeField] private TMP_Text item1QuantityText;
    
    [Header("Ingredient 2")]
    [SerializeField] private Image item2Icon;
    [SerializeField] private TMP_Text item2QuantityText;
    
    [Header("Ingredient 3")]
    [SerializeField] private Image item3Icon;
    [SerializeField] private TMP_Text item3QuantityText;
    
    public void UIShowCraftPopup(ItemRecipe itemRecipe)
    {
        _currentlySelectedRecipe = itemRecipe;
        
        itemToCraftIcon.sprite = itemRecipe.GetItemToCraft().Icon;
        itemToCraftText.text = itemRecipe.GetItemToCraft().Name;
        feedbackMessage.text = "";

        item1Icon.sprite = itemRecipe.GetNecessaryItems()[0].itemData.Icon;
        item1QuantityText.text = itemRecipe.GetNecessaryItems()[0].quantity.ToString();

        if (itemRecipe.GetNecessaryItems().Count >= 2)
        {
            item2Icon.sprite = itemRecipe.GetNecessaryItems()[1].itemData.Icon;
            item2QuantityText.text = itemRecipe.GetNecessaryItems()[1].quantity.ToString();
        }
        else
        {
            item2Icon.sprite = noIngredientSprite;
        }
        
        if (itemRecipe.GetNecessaryItems().Count >= 3)
        {
            item3Icon.sprite = itemRecipe.GetNecessaryItems()[2].itemData.Icon;
            item3QuantityText.text = itemRecipe.GetNecessaryItems()[2].quantity.ToString();
        }
        else
        {
            item3Icon.sprite = noIngredientSprite;
        }
        
        craftPopup.SetActive(true);
    }

    public void CallCraftItem()
    {
        feedbackMessage.text = InventoryManager.Instance.CraftItem(_currentlySelectedRecipe).Message;
    }
}

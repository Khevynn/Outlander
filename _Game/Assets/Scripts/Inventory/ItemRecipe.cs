using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct ItemDataHolderForRecipe
{
    public ItemData itemData;
    public int quantity;
}

[CreateAssetMenu(fileName = "ItemRecipe", menuName = "Scriptable Objects/ItemRecipe")]
public class ItemRecipe : ScriptableObject
{
    [SerializeField] private List<ItemDataHolderForRecipe> necessaryItemsToCraft;
    [SerializeField] private ItemData itemToBeCrafted;
    [SerializeField] private int amountToBeCrafted;

    public List<ItemDataHolderForRecipe> GetNecessaryItems() => necessaryItemsToCraft;
    public ItemData GetItemToCraft() => itemToBeCrafted;
    public int GetAmountToCraft() => amountToBeCrafted;
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct DroppableItem
{
    public Item itemPrefab;
    public int minAmount;
    public int maxAmount;
    public float chanceOfDropping;
}

public class DropItemsComponent : MonoBehaviour
{
    [SerializeField] private List<DroppableItem> itemsToDrop;
    
    public void DropItems()
    {
        for(int i = 0; i < itemsToDrop.Count; ++i)
        {
            if (Random.Range(0f, 100f) <= itemsToDrop[i].chanceOfDropping)
            {
                Item item = ItemsPool.Instance.GetItemWithId(itemsToDrop[i].itemPrefab.GetItemData().Id);
                item.SetAmountOfItems(Random.Range(itemsToDrop[i].minAmount, itemsToDrop[i].maxAmount + 1));

                item.transform.position = transform.position + new Vector3(0f,1f,0f);
                item.gameObject.SetActive(true);
            }
        }
    }
}

using UnityEngine;

public class Item : MonoBehaviour, IInteract
{
    [SerializeField] protected ItemData itemData;
    [SerializeField] protected int amountOfItems;
    
    public virtual void OnInteract()
    {
        InventoryManager.Instance.CollectItem(this);
        ItemsPool.Instance.ReturnItemToPool(this);
        gameObject.SetActive(false);
    }
    
    public void SetItemData(ItemData inItemData) => this.itemData = inItemData;
    public void SetAmountOfItems(int inAmountOfItems) => this.amountOfItems = inAmountOfItems;
    
    public ItemData GetItemData() => itemData;
    public int GetAmountOfItems() => amountOfItems;
}

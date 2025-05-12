using UnityEngine;

public class RocketEngineController : MonoBehaviour, IInteract
{
    [Header("References")]
    [SerializeField] private ItemData necessaryItemToFix;
    
    public void OnInteract()
    {
        var item = InventoryManager.Instance.SearchItemInInventory(necessaryItemToFix.Id);
        if (item)
        {
            GameManager.Instance.Win();
        }
        else
        {
            print("Player doesn't have engine");
        }
    }
}

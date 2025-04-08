using UnityEngine;
using UnityEngine.Serialization;

public class TestItem : InteractableParent
{
    [FormerlySerializedAs("droppedItem")] [FormerlySerializedAs("item")] [SerializeField] private InWorldItem inWorldItem;
    
    public override void Interact()
    {
        InventoryManager.Instance.AddItem(inWorldItem);
    }
}

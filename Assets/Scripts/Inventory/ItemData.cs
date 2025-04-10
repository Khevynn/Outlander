using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemData : ScriptableObject
{
    [SerializeField] private int itemId;
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    [SerializeField] private int maxStack;
    [SerializeField] private Sprite itemIcon;
    
    public int Id => itemId;
    public string Name => itemName;
    public string Description => itemDescription;
    public int MaxStack => maxStack;
    public bool IsStackable => maxStack > 1;
    public Sprite Icon => itemIcon;
}
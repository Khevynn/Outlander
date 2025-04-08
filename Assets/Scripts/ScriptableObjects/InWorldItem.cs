using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class InWorldItem : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    [SerializeField] private bool _isStackable;

    public string Name => _name;
    public Sprite Icon => _icon;
    public string Description => _description;
    public bool IsStackable => _isStackable;
    
}

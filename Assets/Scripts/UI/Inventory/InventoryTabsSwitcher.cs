using UnityEngine;
using UnityEngine.Serialization;

enum Tab
{
    Inventory = 0,
    Crafting = 1
}

public class InventoryTabsSwitcher : MonoBehaviour
{
    [Header("Components")]
    private Animator _animator;

    [FormerlySerializedAs("_currentTab")]
    [Header("Tabs Control")]
    [SerializeField] private Tab currentTab;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void SwitchTabs()
    {
        if (currentTab == Tab.Inventory)
        {
            currentTab = Tab.Crafting;
            _animator.Play("SwitchToCraft");
        }
        else
        {
            currentTab = Tab.Inventory;
            _animator.Play("SwitchToInventory");
        }
    }
}

using System;
using UnityEngine;

public class ItemHoverPopup : MonoBehaviour, IHover
{
    private Item itemInfo;

    private void Start()
    {
        itemInfo = GetComponent<Item>();
    }

    public void OnHoverEnter()
    {
        InGamePopupsController.Instance.ShowItemInfo(itemInfo);
    }
    public void OnHoverExit()
    {
        InGamePopupsController.Instance.HideItemInfoPopup();
    }
}

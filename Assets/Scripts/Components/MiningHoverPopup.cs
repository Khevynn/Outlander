using UnityEngine;

public class MiningHoverPopup : MonoBehaviour, IHover
{
    public void OnHoverEnter()
    {
        InGamePopupsController.Instance.ShowMiningIcon();
    }
    public void OnHoverExit()
    {
        InGamePopupsController.Instance.HideMiningIcon();
    }
}

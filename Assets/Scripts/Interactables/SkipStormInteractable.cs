using UnityEngine;

public class SkipStormInteractable : MonoBehaviour, IInteract
{
    public void OnInteract()
    {
        StormManager.Instance.SkipStorm();
    }
}

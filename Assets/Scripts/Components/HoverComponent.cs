using UnityEngine;
using UnityEngine.Serialization;

public class HoverComponent : MonoBehaviour, IHover
{
    [Header("References")] 
    [SerializeField] private GameObject outlineMeshRenderer;
    
    public virtual void OnHoverEnter()
    {
        outlineMeshRenderer.SetActive(true);
    }
    
    public virtual void OnHoverExit()
    {   
        outlineMeshRenderer.SetActive(false);
    }
}

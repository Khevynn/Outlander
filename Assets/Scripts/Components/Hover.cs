using UnityEngine;
using UnityEngine.Serialization;

public class Hover : MonoBehaviour, IHover
{
    [Header("References")] 
    [SerializeField] private GameObject outlineMeshRenderer;
    
    public virtual void OnHover()
    {
        outlineMeshRenderer.SetActive(true);
    }
    
    public virtual void OnHoverExit()
    {   
        outlineMeshRenderer.SetActive(false);
    }
}

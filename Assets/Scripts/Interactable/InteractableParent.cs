using System.Linq;
using UnityEngine;

public class InteractableParent : MonoBehaviour, IInteractable
{
    [Header("References")] 
    [SerializeField] protected MeshRenderer meshToApplyOutline;
    [SerializeField] protected Material interactableMaterial;
    private Material[] originalMaterials;
    private Material[] newMaterials;

    private void Start()
    {
        if(!meshToApplyOutline) meshToApplyOutline = GetComponent<MeshRenderer>();
        originalMaterials = meshToApplyOutline.materials;
        
        SetupHoverMaterialsArray();
    }
    
    public virtual void OnHover()
    {
        meshToApplyOutline.materials = newMaterials;
    }
    
    public virtual void OnHoverExit()
    {   
        meshToApplyOutline.materials = originalMaterials;
    }
    
    public virtual void Interact()
    {
        // print("Interacted with " + gameObject.name);
    }
    
    private void SetupHoverMaterialsArray()
    {
        newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[^1] = interactableMaterial;
    }
}

using UnityEngine;
using UnityEngine.Serialization;

public class Hover : MonoBehaviour, IHover
{
    [Header("References")] 
    [SerializeField] protected Material onHoverMaterial;
    private MeshRenderer meshToApplyOutline;
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
    
    private void SetupHoverMaterialsArray()
    {
        newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[^1] = onHoverMaterial;
    }
}

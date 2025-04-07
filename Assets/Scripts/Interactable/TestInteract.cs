using UnityEngine;

public class TestInteract : InteractableParent
{
    public override void Interact()
    {
        print("Interacted with " + gameObject.name);
    }
}

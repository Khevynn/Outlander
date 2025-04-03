using UnityEngine;

public class TestInteract : InteractablesParent
{
    public override void Interact()
    {
        print("Interacted with " + gameObject.name);
    }
}

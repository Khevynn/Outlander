using UnityEngine;

[System.Serializable]
public class Save
{
    public static Save instance;
    
    public Save()
    {
        instance = this;
    }
}

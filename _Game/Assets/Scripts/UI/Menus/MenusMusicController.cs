using System;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class MenusMusicController : MonoBehaviour
{
    public static MenusMusicController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("MenusMusicController already exists, destroying new one");
            Destroy();
        }
        else
            Instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}

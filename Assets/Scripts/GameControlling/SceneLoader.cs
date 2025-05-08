using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("SceneLoader already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}

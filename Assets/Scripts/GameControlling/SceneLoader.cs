using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    public string SceneToUnload { get; private set; }
    public string SceneToLoad { get; private set; }

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
        SceneToUnload = SceneManager.GetActiveScene().name;
        SceneToLoad = sceneName;
        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Additive);
    }
}

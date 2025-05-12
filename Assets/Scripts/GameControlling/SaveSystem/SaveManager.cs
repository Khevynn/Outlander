using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public static Save savedGame;

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("SaveManager already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public static void OverwriteSave()
    {
        var binaryFormatter = new BinaryFormatter();
        var file = File.Create(Path.Combine(Application.persistentDataPath, "GameSave.txt"));
        binaryFormatter.Serialize(file, savedGame);
        file.Close();
    }
    public static void NewSave()
    {
        savedGame = new Save();
        var binaryFormatter = new BinaryFormatter();
        var file = File.Create(Path.Combine(Application.persistentDataPath, "GameSave.txt"));
        
        binaryFormatter.Serialize(file, savedGame);
        file.Close();
    }
    public static void LoadSave() 
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, "GameSave.txt")))
        {
            var binaryFormatter = new BinaryFormatter();
            var file = File.Open(Path.Combine(Application.persistentDataPath, "GameSave.txt"), FileMode.Open);
            savedGame = (Save)binaryFormatter.Deserialize(file);
            file.Close();
        }
    }
}

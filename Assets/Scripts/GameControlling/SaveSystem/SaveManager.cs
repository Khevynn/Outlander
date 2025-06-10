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

    public static void StartNewPerformanceGraph()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GamePerformance.txt");

        // Ensure the file exists before appending
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close(); // Close immediately to release the handle
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine("/////////////////////////////////////// STARTED NEW GAME ///////////////////////////////////////");
        }
    }

    public static void WritePerformance(string fps)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GamePerformance.txt");

        // Ensure the file exists before appending
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close(); // Close immediately to release the handle
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine($"{timestamp}:{fps}");
        }
    }
}

using System;
using System.IO;
using UnityEngine;

public class SaveDataManager
{
    private readonly CardLibrary library;
    private const string fileName = "savedata.json";
    private readonly string filePath;
    
    public SaveDataManager(CardLibrary library)
    {
        this.library = library;

        filePath = $"{Application.persistentDataPath}/{fileName}";
    }

    public void Save()
    {
        SaveAsJSON(new SaveData(library), filePath);
    }

    public SaveData Load()
    {
        return LoadAsJSON(filePath);
    }

    private void SaveAsJSON(SaveData data, string path)
    {
        //Note: JsonUtility might cause hangs if save data is massive
        string json = JsonUtility.ToJson(data);
        Debug.Log($"Saving as JSON: " + json);
        
        File.WriteAllText(path, json);
    }

    private SaveData LoadAsJSON(string path)
    {
        SaveData save = null;
        
        if (File.Exists(path))
        {
            string fileContents = File.ReadAllText(path);
            try
            {
                save = JsonUtility.FromJson<SaveData>(fileContents);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save data at {path} could not be read: {e}");
            }
        }

        return save;
    }
}
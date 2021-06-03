using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveDataManager
{
    private readonly CardLibrary library;
    private const string fileName = "savedata.save";
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

    private void SaveBinary(SaveData data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filePath);
        bf.Serialize(file, data);
        file.Close();
    }

    private void LoadBinary()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);
        SaveData save = (SaveData)bf.Deserialize(file);
        file.Close();
    }
    
    private void SaveAsJSON(SaveData data, string path)
    {
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
                Debug.Log("Data could not be read.");
            }
        }

        return save;
    }
}
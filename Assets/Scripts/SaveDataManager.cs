using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveDataManager
{
    private CardLibrary library;
    private const string path = "/savedata.save";

    public SaveDataManager(CardLibrary library)
    {
        this.library = library;
    }

    public void Save()
    {
        Save(new SaveData(library));
    }
    
    private void Save(SaveData data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + path);
        bf.Serialize(file, data);
        file.Close();
    }

    public SaveData Load()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + path, FileMode.Open);
        SaveData save = (SaveData)bf.Deserialize(file);
        file.Close();

        library.Initialize(save);
        
        return save;
    }
}
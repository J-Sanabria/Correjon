// SaveManager.cs
using System.IO;
using UnityEngine;

public static class SaveManager
{
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save_v1.json");

    [System.Serializable]
    public class SaveData
    {
        public int wallet = 0;
        public int journalLeaves = 0;         // # de hojas recolectadas total
        public int[] pagesUnlocked;           // opcional: páginas concretas
    }

    public static SaveData Data { get; private set; } = new SaveData();

    public static void Load()
    {
        if (File.Exists(Path))
        {
            var json = File.ReadAllText(Path);
            Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
        else
        {
            Data = new SaveData();
            Save();
        }
    }

    public static void Save()
    {
        var json = JsonUtility.ToJson(Data);
        File.WriteAllText(Path, json);
    }
}

using System.IO;
using UnityEngine;

public static class SaveManager
{
    // Ruta única para este dispositivo
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    [System.Serializable]
    public class SaveData
    {
        public int wallet = 0;            // monedas totales
        public int journalLeaves = 0;     // hojas del diario
        public int[] pagesUnlocked = new int[0]; // opcional: páginas del diario desbloqueadas
    }

    public static SaveData Data { get; private set; } = new SaveData();

    // Cargar datos al iniciar el juego
    public static void Load()
    {
        if (File.Exists(Path))
        {
            string json = File.ReadAllText(Path);
            Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
        else
        {
            Data = new SaveData();
            Save(); // crea archivo nuevo vacío
        }
    }

    // Guardar datos — una sola versión por dispositivo
    public static void Save()
    {
        string json = JsonUtility.ToJson(Data, prettyPrint: true);
        File.WriteAllText(Path, json);
        Debug.Log($"[SaveManager] Guardado único actualizado en {Path}");
    }
}

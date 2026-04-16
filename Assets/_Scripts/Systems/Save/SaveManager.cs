using System.IO;
using UnityEngine;

/// <summary>
/// Статический класс для записи и чтения файлов сохранений (3 слота).
/// Файлы хранятся в Application.persistentDataPath в формате JSON.
/// </summary>
public static class SaveManager
{
    private static string GetPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static void Save(int slot, float health, float stamina, string sceneName)
    {
        var data = new SaveData
        {
            health = health,
            stamina = stamina,
            sceneName = sceneName
        };

        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
        Debug.Log($"[SaveManager] Slot {slot} saved: HP={health}, Stamina={stamina}, Scene={sceneName}");
    }

    public static SaveData Load(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path))
            return null;

        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public static bool Exists(int slot)
    {
        return File.Exists(GetPath(slot));
    }

    public static void Delete(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] Slot {slot} deleted");
        }
    }
}

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

    public static void Save(int slot, float health, float stamina, string sceneName,
        System.Collections.Generic.IReadOnlyList<InventoryEntry> inventoryEntries = null)
    {
        var data = new SaveData
        {
            health = health,
            stamina = stamina,
            sceneName = sceneName
        };

        if (inventoryEntries != null)
        {
            foreach (var entry in inventoryEntries)
                if (entry?.item != null && !string.IsNullOrEmpty(entry.item.id) && entry.count > 0)
                    data.inventory.Add(new SaveData.SavedItem { itemId = entry.item.id, count = entry.count });
        }

        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
        Debug.Log($"[SaveManager] Slot {slot} saved: HP={health}, Stamina={stamina}, Scene={sceneName}, Items={data.inventory.Count}");
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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public const int SlotCount = 3;

    private static string SaveDir => Path.Combine(Application.persistentDataPath, "saves");

    private static string SlotPath(int slot) =>
        Path.Combine(SaveDir, $"slot_{slot}.json");

    public static void Save(int slot, float health, float stamina, string sceneName,
        IReadOnlyList<InventoryEntry> inventory = null)
    {
        var data = new SaveData
        {
            slot = slot,
            playerHealth = health,
            stamina = stamina,
            sceneName = sceneName,
        };

        if (inventory != null)
            foreach (var entry in inventory)
                data.inventory.Add(new InventoryItemData { itemId = entry.item.id, count = entry.count });

        Directory.CreateDirectory(SaveDir);
        File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data));
    }

    public static SaveData Load(int slot)
    {
        var path = SlotPath(slot);
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public static bool Exists(int slot) => File.Exists(SlotPath(slot));

    public static void Delete(int slot)
    {
        var path = SlotPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
}

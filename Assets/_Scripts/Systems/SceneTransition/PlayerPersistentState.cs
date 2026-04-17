using System.Collections.Generic;

/// <summary>
/// Хранит состояние игрока (здоровье, выносливость, инвентарь) между переходами между сценами.
/// Регистрируется как глобальный сервис в GameServiceRegistry.
/// </summary>
public class PlayerPersistentState
{
    public float Health { get; private set; }
    public float Stamina { get; private set; }
    public bool HasData { get; private set; }

    private List<InventoryEntry> _inventoryEntries;
    public IReadOnlyList<InventoryEntry> InventoryEntries => _inventoryEntries;

    public void Save(float health, float stamina)
    {
        Health = health;
        Stamina = stamina;
        HasData = true;
    }

    public void SaveInventory(IReadOnlyList<InventoryEntry> entries)
    {
        _inventoryEntries = entries != null ? new List<InventoryEntry>(entries) : null;
    }

    public void ClearInventory()
    {
        _inventoryEntries = null;
    }

    public void Clear()
    {
        HasData = false;
    }
}

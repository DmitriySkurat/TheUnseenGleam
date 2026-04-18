using UnityEngine;

public class PlayerInventory : Inventory, ISessionLifecycle
{
    [SerializeField] ItemRegistry _itemRegistry;

    public InitializationOrder Order => InitializationOrder.Player + 10;

    PlayerContext _ctx;

    public void Initialize()
    {
        if (Services.IsRegistered<PlayerContext>())
        {
            _ctx = Services.Get<PlayerContext>();
            _ctx.inventory = this;
        }

        RestoreFromSave();
    }

    public void Dispose() { }

    public void Update()
    {
        if (_ctx == null)
        {
            Debug.LogError("Придурок забыл инициализировать PlayerInventory");
            return;
        }
    }

    public bool TryUse(ItemData item, PlayerContext ctx)
    {
        if (item == null || !item.CanUse) return false;
        if (!Has(item)) return false;

        item.Use(ctx);
        if (item.type == ItemType.Consumable)
            Remove(item, 1);

        return true;
    }

    // Consumes from the specific slot entry so the correct stack is decremented.
    public bool TryUse(InventoryEntry entry, PlayerContext ctx)
    {
        if (entry?.item == null || !entry.item.CanUse) return false;
        if (entry.count <= 0) return false;

        entry.item.Use(ctx);
        if (entry.item.type == ItemType.Consumable)
            Remove(entry, 1);

        return true;
    }

    private void RestoreFromSave()
    {
        var pending = SaveVariables.PendingSave;
        if (pending == null || pending.inventory == null || pending.inventory.Count == 0)
            return;

        // Восстанавливаем только если инвентарь ещё пуст (первый старт сессии из сохранения).
        // При переходе между сценами внутри сессии инвентарь уже заполнен через ISessionLifecycle.
        if (GetEntries().Count > 0)
            return;

        if (_itemRegistry == null)
        {
            Debug.LogWarning("[PlayerInventory] ItemRegistry не назначен — инвентарь из сохранения не восстановлен");
            return;
        }

        foreach (var saved in pending.inventory)
        {
            var itemData = _itemRegistry.GetById(saved.itemId);
            if (itemData != null)
                Add(itemData, saved.count);
            else
                Debug.LogWarning($"[PlayerInventory] Предмет с id '{saved.itemId}' не найден в ItemRegistry");
        }

        SaveVariables.PendingSave = null;
        Debug.Log($"[PlayerInventory] Восстановлен инвентарь: {pending.inventory.Count} видов предметов");
    }
}

using UnityEngine;
using HSM;
using System.Collections.Generic;

public class PlayerInventory : Inventory, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 10;

    PlayerContext _ctx;
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _ctx.inventory = this;

        if (_ctx.inventorySnapshot != null && _ctx.inventorySnapshot.Count > 0)
        {
            items = _ctx.inventorySnapshot;
            _ctx.inventorySnapshot = null;
        }
    }

    public void Dispose()
    {
        _ctx.inventorySnapshot = new System.Collections.Generic.List<InventoryEntry>(items);
    }

    public void Update()
    {
        if (_ctx == null)
        {
            Debug.LogError("Придурок забыл инициализировать PlayerInventory");
            return;
        }
        
        // foreach (Entry item in items)
        // {
        //     Debug.Log($" item name {item.item.name} {item.count}");
        // }
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
}
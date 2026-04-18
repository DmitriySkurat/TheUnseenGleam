using UnityEngine;
using HSM;
using System.Collections.Generic;

public class PlayerInventory : Inventory, ISessionLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 10;

    PlayerContext _ctx;

    public void Initialize()
    {
        if (Services.IsRegistered<PlayerContext>())
        {
            _ctx = Services.Get<PlayerContext>();
            _ctx.inventory = this;
        }
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
}

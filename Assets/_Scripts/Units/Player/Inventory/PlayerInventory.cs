using UnityEngine;
using HSM;
using System.Collections.Generic;

public class PlayerInventory : Inventory, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 10;

    PlayerContext _ctx;
    
    public void Initialize() {
        _ctx = Services.Get<PlayerContext>();
        
        _ctx.inventory = this;
    }
    
    public void Dispose()
    {
        
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
        if (item == null || !item.CanUse)
            return false;

        if (!Has(item))
            return false;

        item.Use(ctx);
        
        if (item.type == ItemType.Consumable) 
            Remove(item, 1);

        return true;
    }
}
using UnityEngine;
using HSM;
using System.Collections.Generic;

public class PlayerInventory : Inventory, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 10;

    PlayerContext _ctx;
    
    public void Initialize() {
        _ctx = Services.Get<PlayerContext>();
        
        _ctx.inventory = this;
    }

    public void Update()
    {
        if (_ctx == null)
        {
            Debug.LogError("Придурок забыл инициализировать PlayerInventory");
            return;
        }
    }
}
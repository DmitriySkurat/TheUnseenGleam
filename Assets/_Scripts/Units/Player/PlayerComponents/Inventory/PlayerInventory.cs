using UnityEngine;
using HSM;
using System.Collections.Generic;

public class PlayerInventory : Inventory, IPlayerComponent
{
    PlayerContext _ctx;
    PlayerStateDriver _driver;
    

    public void Initialize(PlayerContext context, PlayerStateDriver driver) {
        _ctx = context;
        _driver = driver;
        
        _ctx.inventory = this;
    }
}
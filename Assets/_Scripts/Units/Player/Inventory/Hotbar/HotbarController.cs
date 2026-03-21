using UnityEngine;
using System.Collections.Generic;

public class HotbarController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 20;
    
    [SerializeField] private int slotCount = 5;
    
    [SerializeField] private List<InventoryEntry> _slots;
    
    private PlayerContext _ctx;

    public void Initialize()
    {
        Debug.Log($"Hotbar Controller Initialized");
        _ctx = Services.Get<PlayerContext>();

        _slots = new List<InventoryEntry>(slotCount);
        
        var entries = _ctx.inventory.GetEntries();
        
        if (entries.Count > 0)
        {
            foreach (InventoryEntry cell in entries)
            {
                Debug.Log($"item name {cell.item.name} count {cell.count}");
                //AssignItem(0, entries[0].item);
            }
        }
    }

    void Update()
    {
        OnSlotSelected(_ctx.input.SlotPressed);
    }

    private void OnSlotSelected(int index)
    {
        if (index < 0 || index >= _slots.Count)
            return;

        UseSlot(index);
    }

    private void UseSlot(int index)
    {
        var slot = _slots[index];

        if (slot == null)
            return;

        if (!_ctx.inventory.TryUse(slot.item, _ctx))
        {
            Debug.Log("Нет предмета в инвентаре");

            // чистим слот если предмет закончился
            if (!_ctx.inventory.Has(slot.item))
                slot.item = null;
        }
    }

    public void AssignItem(int index, ItemData item)
    {
        if (index < 0 || index >= _slots.Count)
            return;

        if (item == null || !item.CanUse)
        {
            Debug.Log("Нельзя добавить предмет в хотбар");
            return;
        }

        _slots[index].item = item;
    }
}
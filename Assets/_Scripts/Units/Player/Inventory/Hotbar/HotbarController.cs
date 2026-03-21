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
        _ctx = Services.Get<PlayerContext>();

        EnsureSlots();
        RebindSlotsToInventory();
        
        var entries = _ctx.inventory.GetEntries();
        
        if (entries.Count > 0)
        {
            foreach (InventoryEntry cell in entries)
            {
                Debug.Log($"item name {cell.item.name} count {cell.count}");
                AssignItem(cell.item);
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
            Debug.Log("There is no item in inventory");

            // чистим слот если предмет закончился
            if (!_ctx.inventory.Has(slot.item))
                _slots[index] = null;
        }
    }

    public void AssignItem(ItemData item)
    {
        if (item == null || !item.CanUse)
        {
            Debug.Log("Item is non usable or null");
            return;
        }

        EnsureSlots();

        var freeIndex = _slots.FindIndex(s => s == null);
        if (freeIndex < 0)
        {
            Debug.Log("No free hotbar slot");
            return;
        }

        var entry = FindInventoryEntry(item, _ctx.inventory.GetEntries());
        if (entry == null)
        {
            Debug.Log("Item not found in inventory");
            return;
        }

        _slots[freeIndex] = entry;
    }

    private void EnsureSlots()
    {
        if (_slots == null)
            _slots = new List<InventoryEntry>(slotCount);

        if (_slots.Count > slotCount)
            _slots.RemoveRange(slotCount, _slots.Count - slotCount);

        while (_slots.Count < slotCount)
            _slots.Add(null);
    }

    private void RebindSlotsToInventory()
    {
        var entries = _ctx.inventory.GetEntries();
        if (entries == null || entries.Count == 0)
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i] = null;
            return;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot == null || slot.item == null)
            {
                _slots[i] = null;
                continue;
            }

            var rebinding = FindInventoryEntry(slot.item, entries);
            _slots[i] = rebinding;
        }
    }

    private InventoryEntry FindInventoryEntry(ItemData item, IReadOnlyList<InventoryEntry> entries)
    {
        if (item == null || entries == null)
            return null;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry != null && entry.item == item)
                return entry;
        }

        return null;
    }
}

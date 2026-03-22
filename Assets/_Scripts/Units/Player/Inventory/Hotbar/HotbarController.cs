using System.Collections.Generic;
using UnityEngine;

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

        _ctx.selectedHotbarSlot = -1;
        _ctx.selectedHotbarEntry = null;

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

    private void Update()
    {
        if (_ctx == null)
            return;

        OnSlotSelected(_ctx.input.SlotPressed);
        SyncSelectedSlot();
        HandlePrimaryAction();
    }

    private void OnSlotSelected(int pressedSlot)
    {
        if (pressedSlot <= 0)
            return;

        int slotIndex = pressedSlot - 1;
        if (slotIndex < 0 || slotIndex >= _slots.Count)
            return;

        _ctx.selectedHotbarSlot = slotIndex;
        _ctx.selectedHotbarEntry = RebindSlot(slotIndex);
    }

    private void HandlePrimaryAction()
    {
        if (!_ctx.input.AttackDown)
            return;

        var selectedItem = _ctx.SelectedHotbarItem;
        if (selectedItem == null)
            return;

        switch (selectedItem.itemName)
        {
            case ItemName.Pebble:
            case ItemName.Mirror:
                // Pebble and Mirror handle their own input flow.
                break;
            default:
                _ctx.inventory.TryUse(selectedItem, _ctx);
                break;
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

    private void SyncSelectedSlot()
    {
        if (_ctx.selectedHotbarSlot < 0 || _ctx.selectedHotbarSlot >= _slots.Count)
        {
            _ctx.selectedHotbarEntry = null;
            return;
        }

        _ctx.selectedHotbarEntry = RebindSlot(_ctx.selectedHotbarSlot);
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
            _slots[i] = RebindSlot(i);
    }

    private InventoryEntry RebindSlot(int index)
    {
        var slot = _slots[index];
        if (slot == null || slot.item == null)
        {
            _slots[index] = null;
            return null;
        }

        var rebinding = FindInventoryEntry(slot.item, _ctx.inventory.GetEntries());
        _slots[index] = rebinding;
        return rebinding;
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

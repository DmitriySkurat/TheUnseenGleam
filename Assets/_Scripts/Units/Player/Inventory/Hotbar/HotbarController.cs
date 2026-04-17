using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 20;

    [SerializeField] private int slotCount = 5;
    [SerializeField] private List<InventoryEntry> _slots;

    public IReadOnlyList<InventoryEntry> Slots => _slots;
    public int selectedHotbarSlot = -1;
    public InventoryEntry selectedHotbarEntry;
    public ItemData SelectedHotbarItem => selectedHotbarEntry?.item;

    private PlayerContext _ctx;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _ctx.hotbar = this;

        EnsureSlots();
        _ctx.inventory.OnInventoryChanged += HandleInventoryChanged;
        SyncHotbarWithInventory();

        selectedHotbarSlot = -1;
        selectedHotbarEntry = null;
    }

    public void Dispose()
    {
        _ctx.inventory.OnInventoryChanged -= HandleInventoryChanged;
        _ctx.hotbar = null;
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

        selectedHotbarSlot = slotIndex;
        selectedHotbarEntry = RebindSlot(slotIndex);
    }

    private void HandlePrimaryAction()
    {
        if (!_ctx.input.AttackDown)
            return;

        var selectedItem = SelectedHotbarItem;
        if (selectedItem == null)
            return;

        switch (selectedItem.itemName)
        {
            case ItemName.Pebble:
            case ItemName.Mirror:
                // Pebble and Mirror handle their own input flow.
                break;
            default:
                _ctx.inventory.TryUse(selectedHotbarEntry, _ctx);
                break;
        }
    }

    // Returns true if the given item (count units) can fit into existing or free hotbar slots.
    public bool CanFitItem(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return false;

        EnsureSlots();
        int remaining = count;

        // Space in existing slots for this item type
        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot?.item != item) continue;
            remaining -= item.maxStackSize - slot.count;
            if (remaining <= 0) return true;
        }

        // Space in free slots
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null) continue;
            remaining -= item.maxStackSize;
            if (remaining <= 0) return true;
        }

        return remaining <= 0;
    }

    private void HandleInventoryChanged()
    {
        SyncHotbarWithInventory();
    }

    private void SyncSelectedSlot()
    {
        if (selectedHotbarSlot < 0 || selectedHotbarSlot >= _slots.Count)
        {
            selectedHotbarEntry = null;
            return;
        }

        selectedHotbarEntry = RebindSlot(selectedHotbarSlot);
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

    private void SyncHotbarWithInventory()
    {
        if (_ctx?.inventory == null)
            return;

        RebindSlotsToInventory();

        var entries = _ctx.inventory.GetEntries();
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry?.item == null || !entry.item.CanUse) continue;
            if (IsAssigned(entry)) continue;

            var freeIndex = _slots.FindIndex(s => s == null);
            if (freeIndex < 0) break;
            _slots[freeIndex] = entry;
        }

        SyncSelectedSlot();
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

    // Validates a slot's entry still exists in inventory by reference.
    private InventoryEntry RebindSlot(int index)
    {
        var slot = _slots[index];
        if (slot == null || slot.item == null)
        {
            _slots[index] = null;
            return null;
        }

        var entries = _ctx.inventory.GetEntries();
        for (int i = 0; i < entries.Count; i++)
            if (entries[i] == slot) return slot;

        _slots[index] = null;
        return null;
    }

    private bool IsAssigned(InventoryEntry entry)
    {
        for (int i = 0; i < _slots.Count; i++)
            if (_slots[i] == entry) return true;
        return false;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] protected List<InventoryEntry> items = new List<InventoryEntry>();

    public event Action OnInventoryChanged;

    public bool Has(ItemData item, int count = 1)
    {
        if (item == null) return false;
        int total = 0;
        foreach (var e in items)
            if (e.item == item) total += e.count;
        return total >= count;
    }

    public void Add(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return;

        int remaining = count;

        // Fill existing stacks first
        foreach (var e in items)
        {
            if (e.item != item) continue;
            int space = item.maxStackSize - e.count;
            if (space <= 0) continue;
            int toAdd = Mathf.Min(space, remaining);
            e.count += toAdd;
            remaining -= toAdd;
            if (remaining <= 0) break;
        }

        // Create new entries for overflow
        while (remaining > 0)
        {
            int toAdd = Mathf.Min(item.maxStackSize, remaining);
            items.Add(new InventoryEntry { item = item, count = toAdd });
            remaining -= toAdd;
        }

        OnInventoryChanged?.Invoke();
    }

    public bool Remove(ItemData item, int count = 1)
    {
        if (!Has(item, count)) return false;

        int remaining = count;
        for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var e = items[i];
            if (e.item != item) continue;
            int toRemove = Mathf.Min(e.count, remaining);
            e.count -= toRemove;
            remaining -= toRemove;
            if (e.count == 0) items.RemoveAt(i);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // Removes from a specific entry (slot-precise consumption).
    public bool Remove(InventoryEntry entry, int count = 1)
    {
        if (entry == null || count <= 0 || entry.count < count) return false;
        if (!items.Contains(entry)) return false;

        entry.count -= count;
        if (entry.count == 0) items.Remove(entry);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public IReadOnlyList<InventoryEntry> GetEntries() => items;
}

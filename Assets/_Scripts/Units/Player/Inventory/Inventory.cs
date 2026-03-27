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
        var e = items.Find(x => x.item == item);
        return e != null && e.count >= count;
    }

    public void Add(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return;

        var e = items.Find(x => x.item == item);
        if (e == null) items.Add(new InventoryEntry { item = item, count = count });
        else e.count += count;

        OnInventoryChanged?.Invoke();
    }

    public bool Remove(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return false;

        var e = items.Find(x => x.item == item);
        if (e == null || e.count < count) return false;

        e.count -= count;
        if (e.count == 0) items.Remove(e);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public IReadOnlyList<InventoryEntry> GetEntries() => items;
}

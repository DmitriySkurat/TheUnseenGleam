using UnityEngine;
using HSM;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class Entry {
        public ItemData item;
        public int count;
    }
    
    [SerializeField] private List<Entry> items = new List<Entry>();
    
    public bool Has(ItemData item, int count = 1) {
        if (item == null) return false;
        var e = items.Find(x => x.item == item);
        return e != null && e.count >= count;
    }

    public void Add(ItemData item, int count = 1) {
        if (item == null || count <= 0) return;
        var e = items.Find(x => x.item == item);
        if (e == null) items.Add(new Entry { item = item, count = count });
        else e.count += count;
        // OnInventoryChanged мб добавить
    } 

    // true - если удалился предмет
    public bool Remove(ItemData item, int count = 1) {
        if (item == null || count <= 0) return false;
        var e = items.Find(x => x.item == item);
        if (e == null || e.count < count) return false;
        e.count -= count;
        if (e.count == 0) items.Remove(e);
        return true;
    }

    public IReadOnlyList<Entry> GetEntries() => items;
    
}
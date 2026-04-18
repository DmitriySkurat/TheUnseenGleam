using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRegistry", menuName = "Inventory/Item Registry")]
public class ItemRegistry : ScriptableObject
{
    [SerializeField] private ItemData[] _items;

    private Dictionary<string, ItemData> _cache;

    public ItemData GetById(string id)
    {
        if (_cache == null)
            BuildCache();
        _cache.TryGetValue(id, out var result);
        return result;
    }

    private void BuildCache()
    {
        _cache = new Dictionary<string, ItemData>();
        if (_items == null) return;
        foreach (var item in _items)
            if (item != null && !string.IsNullOrEmpty(item.id))
                _cache[item.id] = item;
    }
}

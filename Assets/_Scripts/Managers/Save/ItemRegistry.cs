using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Registry", fileName = "ItemRegistry")]
public class ItemRegistry : ScriptableObject
{
    [SerializeField] ItemData[] _items;

    Dictionary<string, ItemData> _map;

    public ItemData GetById(string id)
    {
        if (_map == null)
        {
            _map = new Dictionary<string, ItemData>();
            foreach (var item in _items)
                if (item != null && !string.IsNullOrEmpty(item.id))
                    _map[item.id] = item;
        }
        return _map.TryGetValue(id, out var result) ? result : null;
    }
}

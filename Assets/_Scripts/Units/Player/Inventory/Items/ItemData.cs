using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Items", order = 1)]
public class ItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite sprite;
    
    public ItemType type;
    
    public virtual bool CanUse =>
        type == ItemType.Equipment 
        || type == ItemType.Consumable;
    
    public virtual void Use(PlayerContext ctx)
    {
        Debug.Log($"Use {itemName}");
    }
}

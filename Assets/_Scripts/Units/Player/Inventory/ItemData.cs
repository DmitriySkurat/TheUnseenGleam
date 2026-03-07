using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Items", order = 1)]
public class ItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite sprite;
}

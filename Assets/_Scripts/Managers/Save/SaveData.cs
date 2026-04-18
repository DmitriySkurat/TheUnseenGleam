using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int slot;
    public float playerHealth;
    public float stamina;
    public string sceneName;
    public List<InventoryItemData> inventory = new List<InventoryItemData>();
}

[Serializable]
public class InventoryItemData
{
    public string itemId;
    public int count;
}

// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Runtime.Serialization.Formatters.Binary;
// using UnityEditor.Overlays;
// using UnityEngine;

// public static class SaveManager 
// {
//     public static string savePath = Application.persistentDataPath + "/save.dat";

//     public static void Save()
//     {
//         BinaryFormatter formatter = new BinaryFormatter();
//         FileStream file = File.Create(savePath);

//         SaveData data = new SaveData();
        
//         // Checkpoint
//         data.checkpointIndex = SaveVariables.checkpointIndex;

//         // Inventory
//         Inventory inv = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Inventory>();
//         if (inv != null)
//         {
//             data.inventoryItems = new string[inv.Items.Count];
//             for (int i = 0; i < inv.Items.Count; i++)
//             {
//                 data.inventoryItems[i] = inv.Items[i].name;

//                 Debug.Log("SavedItem = " + inv.Items[i].name);

//             }
//         }

//         // Health
//         PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
//         if (player != null)
//         {
//             data.playerHealth = player.CurrentHealth;
//         }

//         // Dropped Items
//         GameObject[] droppedObjects = GameObject.FindGameObjectsWithTag("DroppedItem");

//         if (droppedObjects != null)
//         {
//             List<DroppedItemData> droppedList = new List<DroppedItemData>();
//             foreach (GameObject obj in droppedObjects)
//             {
//                 DroppedItem d = obj.GetComponent<DroppedItem>();
//                 if (d == null || d.GetItem() == null) continue;
//                 DroppedItemData dData = new DroppedItemData();
//                 dData.itemName = d.GetItem().name;
//                 dData.x = obj.transform.position.x;
//                 dData.y = obj.transform.position.y;
//                 dData.z = obj.transform.position.z;
//                 droppedList.Add(dData);
//             }
//             data.droppedItems = droppedList.ToArray();
//         }
        

//         formatter.Serialize(file, data);
//         file.Close();
//     }

//     public static void Load()
//     {
//         if (!File.Exists(savePath)) 
//             return;

//         BinaryFormatter formatter = new BinaryFormatter();
//         FileStream file = File.Open(savePath, FileMode.Open);

//         SaveData data = (SaveData)formatter.Deserialize(file);
//         file.Close();

//         // Checkpoint
//         SaveVariables.checkpointIndex = data.checkpointIndex;

//         // Inventory
//         Inventory inv = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Inventory>();

//         if (inv != null)
//         {
//             inv.Clear();

//             if (data.inventoryItems != null)
//             {
//                 foreach (string itemName in data.inventoryItems)
//                 {
//                     Item loadedItem = Resources.Load<Item>("Items/" + itemName);
//                     Debug.Log("ItemName = " + itemName + ", loadedItem = " + loadedItem);
//                     if (loadedItem != null)
//                         inv.AddItem(loadedItem);
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("Inventory not found");
//         }

//         // Health
//         PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
//         if (player != null)
//         {
//             if (data.playerHealth != 0)
//             {
//                 player.currentHealth = data.playerHealth;
//             }
//         }

//         // Dropped Items
//         GameObject[] existing = GameObject.FindGameObjectsWithTag("DroppedItem");
//         foreach (var e in existing)
//         {
//             GameObject.Destroy(e);
//         }
//         GameObject prefab = Resources.Load<GameObject>("Prefabs/PrefabBase");
//         if (prefab == null)
//         {
//             Debug.LogError("BasePrefab not found in Resources/Prefabs");
//             return;
//         }
//         if (data.droppedItems != null)
//         {
//             foreach (var d in data.droppedItems)
//             {
//                 if (string.IsNullOrEmpty(d.itemName)) continue;
//                 Item item = Resources.Load<Item>("Items/" + d.itemName);
//                 if (item == null) continue;
//                 GameObject go = GameObject.Instantiate(prefab, new Vector3(d.x, d.y, d.z), Quaternion.identity);
//                 DroppedItem di = go.GetComponent<DroppedItem>();
//                 if (di != null)
//                 {
//                     di.SetItem(item);
//                 }
//             }
//         }
//     }

//     public static void DeleteSave()
//     {
//         if (File.Exists(savePath))
//             File.Delete(savePath);
        
//         // Checkpoint
//         SaveVariables.checkpointIndex = 0;

//         // Inventory
//         Inventory inv = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Inventory>();
//         if (inv != null)
//             inv.Clear();

//         // Health
//         PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
//         if (player != null)
//         {
//             player.currentHealth = player.MaxHealth;
//         }

//         // Dropped Items
//         GameObject[] existing = GameObject.FindGameObjectsWithTag("DroppedItem");
//         foreach (var e in existing)
//         {
//             GameObject.Destroy(e);
//         }
//     }
// }
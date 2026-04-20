using UnityEditor;
using UnityEngine;

public static class SaveManagerEditor
{
    [MenuItem("Tools/Delete All Saves")]
    private static void DeleteAllSaves()
    {
        for (int i = 1; i <= SaveManager.SlotCount; i++)
            SaveManager.Delete(i);
        Debug.Log("All saves deleted.");
    }
}

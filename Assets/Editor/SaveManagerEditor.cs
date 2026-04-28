using UnityEditor;
using UnityEngine;

public static class SaveManagerEditor
{
    [MenuItem("Tools/Delete All Saves", false, 100)]
    private static void DeleteAllSaves()
    {
        for (int i = 1; i <= SaveManager.SlotCount; i++)
            SaveManager.Delete(i);
        Debug.Log("All saves deleted.");
    }
}

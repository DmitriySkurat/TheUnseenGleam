using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public float health;
    public float stamina;
    public string sceneName;
    public List<SavedItem> inventory = new List<SavedItem>();

    [System.Serializable]
    public class SavedItem
    {
        public string itemId;
        public int count;
    }
}

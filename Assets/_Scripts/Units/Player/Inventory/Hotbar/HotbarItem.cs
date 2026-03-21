[System.Serializable]
public class HotbarItem
{
    public ItemData item;

    public bool IsEmpty => item == null;
}
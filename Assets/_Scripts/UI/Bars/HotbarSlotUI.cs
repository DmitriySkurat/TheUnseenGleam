using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Text _countText;
    [SerializeField] private Text _hotkeyText;

    public void Setup(int slotIndex)
    {
        if (_hotkeyText != null)
            _hotkeyText.text = (slotIndex + 1).ToString();
    }

    public void SetEntry(InventoryEntry entry)
    {
        bool hasItem = entry?.item != null;

        _itemImage.sprite = hasItem ? entry.item.sprite : null;
        _itemImage.enabled = hasItem;

        bool showCount = hasItem && entry.count > 1;
        if (_countText != null)
        {
            _countText.text = showCount ? entry.count.ToString() : string.Empty;
            _countText.enabled = showCount;
        }
    }

    public void SetSelected(bool selected, Color selectedColor, Color defaultColor)
    {
        if (_background != null)
            _background.color = selected ? selectedColor : defaultColor;
    }
}

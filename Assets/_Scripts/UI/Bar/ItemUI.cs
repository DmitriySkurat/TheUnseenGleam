using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq.Expressions;

public class ItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image itemImage; // Изображение предмета (ItemIMG)
    [SerializeField] private Image backgroundImage; // Фон слота (BG)

    public Item currentItem { get; private set; }

    public void SetItem(Item item)
    {
        currentItem = item;
        if (item != null && itemImage != null)
        {
            itemImage.sprite = item.sprite;
            itemImage.enabled = true;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        currentItem = null;

        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
    }

    public void SetSelected(bool isSelected, Color selectedColor, Color defaultColor)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : defaultColor;
        }
    }
}

using System;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private ItemUI[] slots = new ItemUI[3];

    [Header("Dependencies")]
    [SerializeField] private Inventory inventory; // Ссылка на инвентарь

    [Header("Selection Settings")]
    [SerializeField] private Color selectedColor = Color.blue; 
    [SerializeField] private Color defaultColor = Color.white;

    public int selectedItemIndex { get; private set; } = 0;

    public event Action OnSelectionChanged;


    private void Awake()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory reference is missing in InventoryUI!");
            return;
        }

        // Подписываемся на изменения
        inventory.OnInventoryChanged += UpdateUI;

        OnSelectionChanged += UpdateSelectionUI;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateUI;

        OnSelectionChanged -= UpdateSelectionUI;
    }

    private void Start()
    {
        UpdateUI(); // Начальное обновление
        selectedItemIndex = 0;
        UpdateSelectionUI(); // Начальное обновление выбора
    }

    private void Update()
    {
        // Обработка нажатий клавиш 1, 2, 3
        if (InputSystem.SelectSlot1() && slots.Length >= 1)
        {
            SetSelectedIndex(0);
        }
        else if (InputSystem.SelectSlot2() && slots.Length >= 2)
        {
            SetSelectedIndex(1);
        }
        else if (InputSystem.SelectSlot3() && slots.Length >= 3)
        {
            SetSelectedIndex(2);
        }

        // Обработка прокрутки колеса мыши
        int scrollDir = InputSystem.ScrollDirection();
        if (scrollDir != 0 && slots.Length > 0)
        {
            int newIndex = (selectedItemIndex + scrollDir + slots.Length) % slots.Length;
            SetSelectedIndex(newIndex);
        }
    }

    private void SetSelectedIndex(int newIndex)
    {
        if (newIndex == selectedItemIndex) return;

        selectedItemIndex = newIndex;
        OnSelectionChanged?.Invoke();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.CurrentCount)
            {
                slots[i].SetItem(inventory.Items[i]);
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    private void UpdateSelectionUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetSelected(i == selectedItemIndex, selectedColor, defaultColor);
        }
    }
}
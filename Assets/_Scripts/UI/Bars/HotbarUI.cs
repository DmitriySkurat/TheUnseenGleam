using UnityEngine;

public class HotbarUI : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [SerializeField] private HotbarSlotUI[] _slots;
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private Color _defaultColor = Color.white;

    private PlayerContext _ctx;
    private HotbarController _hotbar;
    private int _lastSelectedSlot = int.MinValue;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _hotbar = Object.FindFirstObjectByType<HotbarController>(FindObjectsInactive.Include);

        if (_hotbar == null)
            Debug.LogError("[HotbarUI] HotbarController not found in scene");
        if (_slots == null || _slots.Length == 0)
            Debug.LogError("[HotbarUI] Slots array is empty — assign HotbarSlotUI children in Inspector");

        for (int i = 0; i < _slots.Length; i++)
            _slots[i].Setup(i);

        _ctx.inventory.OnInventoryChanged += RefreshItems;
        RefreshItems();
    }

    public void Dispose()
    {
        if (_ctx?.inventory != null)
            _ctx.inventory.OnInventoryChanged -= RefreshItems;
    }

    private void Update()
    {
        if (_ctx == null)
            return;

        int selected = _ctx.selectedHotbarSlot;
        if (selected == _lastSelectedSlot)
            return;

        _lastSelectedSlot = selected;
        for (int i = 0; i < _slots.Length; i++)
            _slots[i].SetSelected(i == selected, _selectedColor, _defaultColor);
    }

    private void RefreshItems()
    {
        if (_hotbar == null)
            return;

        var hotbarSlots = _hotbar.Slots;
        int count = Mathf.Min(_slots.Length, hotbarSlots.Count);
        for (int i = 0; i < count; i++)
            _slots[i].SetEntry(hotbarSlots[i]);
    }
}

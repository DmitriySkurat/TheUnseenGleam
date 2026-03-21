using UnityEngine;

public class HotbarController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 20;

    [SerializeField] private int slotCount = 5;

    private HotbarItem[] _slots;
    
    private PlayerContext _ctx;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();

        _slots = new HotbarItem[slotCount];
        for (int i = 0; i < slotCount; i++)
            _slots[i] = new HotbarItem();
    }

    void Update()
    {
        OnSlotSelected(_ctx.input.SlotPressed);
    }

    private void OnSlotSelected(int index)
    {
        if (index < 0 || index >= _slots.Length)
            return;

        UseSlot(index);
    }

    private void UseSlot(int index)
    {
        var slot = _slots[index];

        if (slot.IsEmpty)
            return;

        if (!_ctx.inventory.TryUse(slot.item, _ctx))
        {
            Debug.Log("Нет предмета в инвентаре");

            // чистим слот если предмет закончился
            if (!_ctx.inventory.Has(slot.item))
                slot.item = null;
        }
    }

    public void AssignItem(int index, ItemData item)
    {
        if (index < 0 || index >= _slots.Length)
            return;

        if (item == null || !item.CanUse)
        {
            Debug.Log("Нельзя добавить предмет в хотбар");
            return;
        }

        _slots[index].item = item;
    }
}
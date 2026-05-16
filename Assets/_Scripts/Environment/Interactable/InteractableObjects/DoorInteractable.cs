using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;

    public bool IsOpen => isOpen;

    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private Sprite closedDoorSprite;

    [SerializeField] private bool disableColliderWhenOpen = true;

    [Header("Vision Blocking")]
    [Tooltip("Отдельный коллайдер на слое окклюзии (дочерний объект). Включён когда дверь закрыта, выключен когда открыта.")]
    [SerializeField] private Collider2D visionBlocker;

    private Collider2D _doorCollider;

    public override void Initialize() {
        base.Initialize();

        _doorCollider = GetComponent<Collider2D>();

        // Для начального состояния
        UpdateDoorVisuals();
    }
    public void SetOpen(bool state)
    {
        if (isOpen == state) return;

        isOpen = state;
        UpdateDoorVisuals();
    }

    private void UpdateDoorVisuals() {
        if (_sr != null) {
            if (isOpen && openDoorSprite != null) _sr.sprite = openDoorSprite;
            else if (!isOpen && closedDoorSprite != null) _sr.sprite = closedDoorSprite;
        }

        if (_doorCollider != null && disableColliderWhenOpen) {
            _doorCollider.isTrigger = isOpen;
            // не сработает с - _doorCollider.enabled = !isOpen;
        }

        if (visionBlocker != null)
            visionBlocker.enabled = !isOpen;
    }

    public override void OnInteract(Interactor interactor)
    {
        isOpen = !isOpen;
        UpdateDoorVisuals();
        Debug.Log(isOpen ? "Door Opened" : "Door Closed");
    }

    protected override void OnAfterInteraction(PlayerInteractor player)
    {
        if (wasItemsConsumed) return;

        base.OnAfterInteraction(player);
    }

    public override void Select() {
        base.Select();
        // Доп логика для подсветки?
    }
}

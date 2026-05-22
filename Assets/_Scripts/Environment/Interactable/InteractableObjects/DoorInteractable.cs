using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [Header("Locked Hint (optional)")]
    [Tooltip("Если задано, игрок всегда может «попробовать» открыть дверь. При нехватке предметов bubble покажет подсказку.")]
    [SerializeField] private SpeechBubble lockedHintBubble;
    [SerializeField, TextArea] private string lockedHintText = "Мне нужен ключ...";

    private Collider2D _doorCollider;
    private ShadowCaster2D _shadowCaster;

    public override void Initialize() {
        base.Initialize();

        _doorCollider = GetComponent<Collider2D>();
        _shadowCaster = GetComponent<ShadowCaster2D>();

        UpdateDoorVisuals();
    }
    public void SetOpen(bool state)
    {
        if (isOpen == state) return;

        isOpen = state;
        UpdateDoorVisuals();
    }

    private void UpdateDoorVisuals() {
        // Включаем до смены спрайта — провайдер ShadowCaster2D обновит кэш формы при изменении
        if (!isOpen && _shadowCaster != null)
            _shadowCaster.enabled = true;

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

        if (isOpen && _shadowCaster != null)
            _shadowCaster.enabled = false;
    }

    public override bool CanBeInteractedBy(Interactor interactor)
    {
        // Если задана подсказка — всегда пропускаем в OnInteract, чтобы показать bubble.
        if (lockedHintBubble != null && interactor is PlayerInteractor)
            return true;
        return base.CanBeInteractedBy(interactor);
    }

    public override void OnInteract(Interactor interactor)
    {
        if (lockedHintBubble != null && !wasItemsConsumed && interactor is PlayerInteractor player && !base.CanBeInteractedBy(player))
        {
            lockedHintBubble.Say(lockedHintText);
            return;
        }

        isOpen = !isOpen;
        UpdateDoorVisuals();
        Debug.Log(isOpen ? "Door Opened" : "Door Closed");
    }

    protected override void OnAfterInteraction(PlayerInteractor player)
    {
        if (wasItemsConsumed) return;
        if (lockedHintBubble != null && !base.CanBeInteractedBy(player)) return;

        base.OnAfterInteraction(player);
    }

    public override void Select() {
        base.Select();
        // Доп логика для подсветки?
    }
}

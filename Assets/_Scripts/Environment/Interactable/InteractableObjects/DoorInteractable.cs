using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isUnlocked = false;
    
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private Sprite closedDoorSprite;

    [SerializeField] private bool disableColliderWhenOpen = true;
    
    [Header("Consume Settings")]
    [Tooltip("Whether to delete required items after interaction")]
    [SerializeField] private bool consumeRequiredItems = true;

    private Collider2D _doorCollider;
    

    void Start() {
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
    
    public override bool CanBeInteractedBy(Interactor interactor)
    {
        if (isUnlocked)
            return true;

        return base.CanBeInteractedBy(interactor);
    }

    public override void Interact(Interactor interactor)
    {
        if (interactor is PlayerInteractor p) 
        {
            var inv = p.Context?.inventory;
            if (inv == null) return;
            
            if (consumeRequiredItems && requiredItems != null) {
                foreach (var req in requiredItems) {
                    if (req.item == null) continue;
                    inv.Remove(req.item, req.count);
                }
            }
            
            isUnlocked = true;
        }
    
        isOpen = !isOpen;
        UpdateDoorVisuals();
        Debug.Log(isOpen ? "Door Opened" : "Door Closed");
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

        
    }

    public override void Select() {
        base.Select();
        // Доп логика для подсветки?
    }
}
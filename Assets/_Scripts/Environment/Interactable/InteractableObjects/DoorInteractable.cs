using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private Sprite closedDoorSprite;

    [SerializeField] private bool disableColliderWhenOpen = true;

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

    public override void Interact(Interactor interactor)
    {
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
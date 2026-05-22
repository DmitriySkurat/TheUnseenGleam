using UnityEngine;

public class PrisonCellDoor : DoorInteractable
{
    [Header("On Open: Permanent Changes")]
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private SortingOrderSetter[] barsOrderSetters;
    [SerializeField] private SortingOrder openedSortingOrder = SortingOrder.InteractablesInBack;

    private bool _cellOpened;

    public override void OnInteract(Interactor interactor)
    {
        bool wasOpen = IsOpen;
        base.OnInteract(interactor);

        if (!_cellOpened && !wasOpen && IsOpen)
        {
            ApplyCellOpenedChanges();
            _cellOpened = true;
        }
    }

    private void ApplyCellOpenedChanges()
    {
        foreach (var col in collidersToDisable)
            if (col != null) col.enabled = false;

        foreach (var setter in barsOrderSetters)
            if (setter != null) setter.SetOrder((int)openedSortingOrder);
    }
}

using UnityEngine;

public class PrisonCellDoor : DoorInteractable
{
    [Header("On Open: Permanent Changes")]
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private Renderer[] barsRenderers;
    [SerializeField] private string openedSortingLayer = "InteractablesInBack";

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

        foreach (var r in barsRenderers)
            if (r != null) r.sortingLayerName = openedSortingLayer;
    }
}

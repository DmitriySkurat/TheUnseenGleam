using UnityEngine;

public class HidingSpotInteractable : Interactable
{
    //public override bool IsComplex => true;  
    public override void OnInteract(Interactor interactor)
    {
        
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}
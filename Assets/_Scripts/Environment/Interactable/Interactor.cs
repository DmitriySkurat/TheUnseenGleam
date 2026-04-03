using UnityEngine;


public class Interactor : MonoBehaviour {
    protected IInteractable currentInteractable;
    protected bool enableHighlight = true;
    
    public IInteractable CurrentInteractable => currentInteractable;

    protected void SetCurrentInteractable(IInteractable interactable) {
        if (ReferenceEquals(currentInteractable, interactable)) return;
        
        if (currentInteractable != null && enableHighlight) currentInteractable.Unselect();
        currentInteractable = interactable;
        if (currentInteractable != null && enableHighlight) currentInteractable.Select();
    }

    protected virtual void PerformInteraction() {
        if (currentInteractable == null) return;
        //if (currentInteractable.IsComplex) return;
        
        currentInteractable.TryInteract(this);
    }
}

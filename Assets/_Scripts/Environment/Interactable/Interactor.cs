using UnityEngine;


public class Interactor : MonoBehaviour {
    protected IInteractable currentInteractable;
    
    public IInteractable CurrentInteractable => currentInteractable;

    protected void SetCurrentInteractable(IInteractable interactable) {
        if (ReferenceEquals(currentInteractable, interactable)) return;
        
        if (currentInteractable != null) currentInteractable.Unselect();
        currentInteractable = interactable;
        if (currentInteractable != null) currentInteractable.Select();
    }

    protected virtual void PerformInteraction() {
        if (currentInteractable == null) return;
        if (currentInteractable.IsComplex) return;
        
        currentInteractable.Interact(this);
    }
}
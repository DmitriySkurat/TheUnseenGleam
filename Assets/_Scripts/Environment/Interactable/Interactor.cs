using UnityEngine;


public class Interactor : MonoBehaviour {
    protected IInteractable currentInteractable;
    
    public IInteractable CurrentInteractable => currentInteractable;

    public void SetCurrentInteractable(IInteractable interactable) {
        if (ReferenceEquals(currentInteractable, interactable)) return;
        
        if (currentInteractable != null) currentInteractable.Unselect();
        currentInteractable = interactable;
        if (currentInteractable != null) currentInteractable.Select();
    }

    public void PerformInteraction() {
        if (currentInteractable == null) return;
        currentInteractable.Interact(this);
    }
}
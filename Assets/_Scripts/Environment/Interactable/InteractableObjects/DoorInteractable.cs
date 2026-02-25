using UnityEngine;

public class DoorInteractable : Interactable
{
    public override void Interact(Interactor interactor)
    {
        Debug.Log("Door interacted with!");
        // Здесь можно добавить логику открытия двери, проигрывания анимации и т.д.
    }
}
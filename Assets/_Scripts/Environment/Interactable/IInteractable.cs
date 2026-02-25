using UnityEngine;

public interface IInteractable {
        string InteractionPrompt { get; }
        void OnFocus();      // игрок "прицелился" на объект
        void OnDefocus();    // игрок отвёл взгляд
        void Interact(GameObject interactor); // вызов взаимодействия
}
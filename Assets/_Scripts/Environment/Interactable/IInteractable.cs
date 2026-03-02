using UnityEngine;

public interface IInteractable {
        void Select(); // Подсветка вкл
        void Unselect(); // Подсветка выкл
        void TryInteract(Interactor interactor);
        void OnInteract(Interactor interactor); 

        bool CanBeInteractedBy(Interactor interactor);
        
        string GetMissingItemsString(Interactor interactor);

        // Скорее всего удалю
        string InteractionPrompt { get; }


        // Нужно ли переходить в состояние HSM
        bool IsComplex { get; }
}
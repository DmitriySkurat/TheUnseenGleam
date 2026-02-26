using UnityEngine;

public interface IInteractable {
        void Select(); // Подсветка вкл
        void Unselect(); // Подсветка выкл
        void Interact(Interactor interactor); 
        
        
        // Скорее всего удалю
        string InteractionPrompt { get; }
        
        
        // Нужно ли переходить в состояние HSM
        bool IsComplex { get; }
}
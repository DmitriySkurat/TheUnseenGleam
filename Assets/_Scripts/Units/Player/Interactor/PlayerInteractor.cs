using System.Linq;
using UnityEngine;
using TMPro;
using NUnit.Framework;
using HSM;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : Interactor, ISceneLifecycle
 {
    public InitializationOrder Order => InitializationOrder.Player + 10;


    [Header("Detection")]
    public float interactRadius = 1.2f;
    public LayerMask interactableLayer;
    public Transform interactOrigin;
    

    [Header("UI")]
    public TextMeshProUGUI promptText;

    private PlayerContext _ctx;
    public PlayerContext Context => _ctx;
    
    public void Initialize() {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();
        
        if (interactOrigin == null) 
            interactOrigin = transform;
    }
    
    public void Dispose()
    {
        
    }

    void Update() {
        if (_ctx == null)
        {
            Debug.LogError("Придурок забыл инициализировать PlayerInteractor");
            return;
        }
        
        ScanForInteractable();

        UpdatePromptUI();
        
        if (_ctx.input.InteractDown && _ctx.CanInteract)
            AttemptInteract();
    }

    void ScanForInteractable() {
        var hits = Physics2D.OverlapCircleAll(interactOrigin.position, interactRadius, interactableLayer);
        
        IInteractable nearest = null;
        float best = float.MaxValue;

        foreach (var col in hits) {
            // ищем компонент, реализующий IInteractable
            var interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector2.SqrMagnitude(((MonoBehaviour)interactable).transform.position - interactOrigin.position);
            if (dist < best) { best = dist; nearest = interactable; }
        }
        
        SetCurrentInteractable(nearest);
    }

    private void AttemptInteract() {
        if (currentInteractable == null) return;
        
        if (!currentInteractable.CanBeInteractedBy(this)) {
            var missingItems = currentInteractable.GetMissingItemsString(this);
            Debug.Log($"Cannot interact, missing items: {missingItems}");
            
            if (promptText != null) 
            {
                promptText.text = $"Missing: {missingItems}";
            }
            
            return;
        }    
    
        PerformInteraction();
            
        _ctx.timeLastInteraction = Time.time;
        
        
        
        // Логика переделывается и переносится в HSM 
        // Здесь пока только логика для isComplex = false, в HSM будем проверять нажата ли Interact и IsComplex = true

        // // Если объект сложный — просим драйвер сменить состояние HSM
        // if (currentInteractable.IsComplex) {
        //     _driver.RequestInteractionState(currentInteractable);
        // } else {
        //     // Если простой — вызываем базовый метод
        //     PerformInteraction();
        // }
    }

    private void UpdatePromptUI() {
        if (promptText == null) return;
        if (currentInteractable == null) {
            promptText.text = "";
            return;
        }

        string basePrompt = currentInteractable.InteractionPrompt;
        string suffix = "";

        if (currentInteractable is Interactable interactable) {
            if (!interactable.CanBeInteractedBy(this)) {
                var miss = interactable.GetMissingItemsString(this);
                suffix = $"\n(Требуется: {miss})";
            }
        }

        promptText.text = basePrompt + suffix;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        var origin = interactOrigin != null ? interactOrigin.position : transform.position;
        Gizmos.DrawWireSphere(origin, interactRadius);
    }
}
using System.Linq;
using UnityEngine;
using TMPro;
using NUnit.Framework;
using HSM;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : Interactor, IPlayerComponent {
    [Header("Detection")]
    public float interactRadius = 1.2f;
    public LayerMask interactableLayer;
    public Transform interactOrigin;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    private PlayerContext _ctx;
    private PlayerStateDriver _driver;
    
    public void Initialize(PlayerContext context, PlayerStateDriver driver) {
        _ctx = context;
        _driver = driver;
    }


    void Awake() {
        if (interactOrigin == null) 
            interactOrigin = transform;
    }

    void Update() {
        if (_ctx == null || _driver == null)
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
        PerformInteraction();
            
        _ctx.timeLastInteraction = _ctx.time;
        
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
        promptText.text = currentInteractable != null ? currentInteractable.InteractionPrompt : "";
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        var origin = interactOrigin != null ? interactOrigin.position : transform.position;
        Gizmos.DrawWireSphere(origin, interactRadius);
    }
}
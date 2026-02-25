using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour, IInteractable {
    // Скорее всего удалю
    [TextArea] public string interactionPrompt = "Press E to interact";
    public virtual string InteractionPrompt => interactionPrompt;
    

    [Header("Visuals")]
    // Включать ли подсветку при фокусе
    public bool highlightOnFocus = true;
    public Color highlightColor = Color.yellow;
    
    private SpriteRenderer _sr;
    private Color _defaultColor;

    public virtual bool IsComplex => false;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        
        if (_sr == null) return;
        
        _defaultColor = _sr.color;
    }

    public virtual void Select() {
        if (!highlightOnFocus || _sr == null) return;
        _sr.color = highlightColor;
    }

    public virtual void Unselect() {
        if (!highlightOnFocus || _sr == null) return;
        _sr.color = _defaultColor;
    }

    public abstract void Interact(Interactor interactor);
}
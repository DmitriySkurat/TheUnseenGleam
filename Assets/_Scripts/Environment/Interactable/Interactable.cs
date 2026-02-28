using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour, IInteractable {
    // Скорее всего удалю
    [TextArea] public string interactionPrompt = "Press E to interact";
    public virtual string InteractionPrompt => interactionPrompt;
    

    [Header("Visuals")]
    public bool highlightOnFocus = true; // Включать ли подсветку при фокусе
    public Color highlightColor = Color.yellow;
    
    protected SpriteRenderer _sr;
    private Color _defaultColor;

    public virtual bool IsComplex => false;
    
    [System.Serializable]
    public struct ItemRequirement {
        public ItemData item;
        [Min(1)]
        public int count;
    }
    
    [Header("Requirements (optional)")]
    public List<ItemRequirement> requiredItems = new List<ItemRequirement>();

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
    
    // Проверка, может ли данный Interactor выполнить взаимодействие (наличие предметов)
    public virtual bool CanBeInteractedBy(Interactor interactor) {
        if (requiredItems == null || requiredItems.Count == 0) return true; // ничего не нужно
        if (interactor is PlayerInteractor player) {
            var inv = player.Context?.inventory;
            if (inv == null) return false;
            foreach (var req in requiredItems) {
                if (req.item == null) continue;
                if (!inv.Has(req.item, req.count)) return false;
            }
            return true;
        }
        // если интерактор не игрок — по умолчанию запрещаем (или разрешаем — на ваш выбор)
        return false;
    }

    // Удобная строка с отсутствующими предметами (для UI)
    public virtual string GetMissingItemsString(Interactor interactor) {
        if (requiredItems == null || requiredItems.Count == 0) return "";
        if (!(interactor is PlayerInteractor player)) return "";
        var inv = player.Context?.inventory;
        if (inv == null) return "";
        var missing = new List<string>();
        foreach (var req in requiredItems) {
            if (req.item == null) continue;
            int have = 0;
            var e = inv.GetEntries();
            foreach (var en in e) { if (en.item == req.item) { have = en.count; break; } }
            if (have < req.count) missing.Add($"{req.item.itemName} x{req.count - have}");
        }
        return string.Join(", ", missing);
    }

    public abstract void Interact(Interactor interactor);
}
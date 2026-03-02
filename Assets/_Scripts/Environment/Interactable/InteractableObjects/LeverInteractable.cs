using System;
using UnityEngine;
using UnityEngine.Events;

public class LeverInteractable : Interactable {
    [SerializeField] private bool isOn = false;

    [SerializeField] private Sprite leverOnSprite;
    [SerializeField] private Sprite leverOffSprite;
    
    
    public UnityEvent<bool> onLeverToggle;
    
    
    void Start() {
        UpdateVisuals();
    }
    
    public override void OnInteract(Interactor interactor)
    {
        isOn = !isOn;

        UpdateVisuals();

        onLeverToggle?.Invoke(isOn);

        Debug.Log($"Lever toggled: {isOn}");
    }
    
    private void UpdateVisuals() {
        if (_sr == null) return;
        
        if (isOn && leverOnSprite != null) _sr.sprite = leverOnSprite;
        else if (!isOn && leverOffSprite != null) _sr.sprite = leverOffSprite;
    }
}

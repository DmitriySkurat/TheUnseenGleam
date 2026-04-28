using UnityEngine;

public class SunflowerInteractable : Interactable
{
    [Header("Sunflower")]
    [Tooltip("Спрайт после сбора подсолнуха")]
    [SerializeField] private Sprite _collectedSprite;

    [Tooltip("Директор спавна агентов — вызывается при первом сборе любого подсолнуха")]
    [SerializeField] private AgentSpawnDirector _agentSpawnDirector;

    private bool _collected;

    public override bool CanBeInteractedBy(Interactor interactor)
    {
        if (_collected) return false;
        return base.CanBeInteractedBy(interactor);
    }

    public override void OnInteract(Interactor interactor)
    {
        if (_collected) return;
        if (interactor is not PlayerInteractor) return;

        _collected = true;

        if (_sr != null && _collectedSprite != null)
            _sr.sprite = _collectedSprite;

        Unselect(); // сбрасываем подсветку

        _agentSpawnDirector?.Activate();
    }
}

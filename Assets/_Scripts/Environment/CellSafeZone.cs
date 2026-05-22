using UnityEngine;

/// <summary>
/// Trigger zone that prevents all agents from detecting the player while they're inside.
/// Attach to a trigger Collider2D covering the cell area.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CellSafeZone : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Interactable;

    private PlayerContext _playerContext;

    public void Initialize()
    {
        if (Services.IsRegistered<PlayerContext>())
            _playerContext = Services.Get<PlayerContext>();
    }

    public void Dispose()
    {
        if (_playerContext != null)
            _playerContext.isInDetectionSafeZone = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_playerContext == null) return;
        if (other.CompareTag("Player"))
            _playerContext.isInDetectionSafeZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_playerContext == null) return;
        if (other.CompareTag("Player"))
            _playerContext.isInDetectionSafeZone = false;
    }
}

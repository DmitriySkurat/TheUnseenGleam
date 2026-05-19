using UnityEngine;

/// <summary>
/// Зона-углубление: игрок, прижавшийся к стене внутри, скрыт от врагов вне зависимости от света.
/// Враг всё равно обнаружит игрока, если войдёт в радиус detectionRadius своего AgentVision.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NicheZone : MonoBehaviour
{
    private PlayerContext _ctx;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Services.IsRegistered<PlayerContext>())
            _ctx = Services.Get<PlayerContext>();
        if (_ctx != null)
            _ctx.isInNiche = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_ctx != null)
            _ctx.isInNiche = false;
        _ctx = null;
    }

    private void OnDestroy()
    {
        if (_ctx != null)
            _ctx.isInNiche = false;
    }
}

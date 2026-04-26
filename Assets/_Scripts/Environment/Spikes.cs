using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    [SerializeField] private float _damage = 9999f;

    private bool _playerInside;
    private float _timer;
    private PlayerContext _ctx;
    private float _savedSpeedMultiplier;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Services.IsRegistered<PlayerContext>())
            _ctx = Services.Get<PlayerContext>();

        _playerInside = true;
        _timer = 0f;

        if (_ctx != null)
        {
            _savedSpeedMultiplier = _ctx.currentSpeedMultiplier;
            _ctx.currentSpeedMultiplier = _ctx.stats.SpikesSpeedMultiplier;
        }

        DamagePlayer();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_ctx != null)
            _ctx.currentSpeedMultiplier = _savedSpeedMultiplier;

        _playerInside = false;
        _ctx = null;
    }

    private void Update()
    {
        if (!_playerInside || _ctx == null)
            return;

        float interval = _ctx.currentFootstepInterval;
        if (interval <= 0f)
            return;

        _timer += Time.deltaTime;
        if (_timer < interval)
            return;

        _timer = 0f;
        DamagePlayer();
    }

    private void DamagePlayer()
    {
        _ctx?.health?.TakeDamage(_damage);
    }
}

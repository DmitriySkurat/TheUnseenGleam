using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    [SerializeField] private float _damage = 9999f;

    private PlayerContext _ctx;

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
        {
            _ctx.isOnSpikes = true;
            _ctx.spikesDamage = _damage;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_ctx != null)
        {
            _ctx.isOnSpikes = false;
            _ctx.spikesDamage = 0f;
        }

        _ctx = null;
    }

    private void OnDestroy()
    {
        if (_ctx != null)
        {
            _ctx.isOnSpikes = false;
            _ctx.spikesDamage = 0f;
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    [SerializeField] private float _damage = 9999f;

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
            Services.Get<PlayerContext>().health?.TakeDamage(_damage);
    }
}

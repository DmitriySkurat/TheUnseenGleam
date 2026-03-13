using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NoiseOnImpact : MonoBehaviour
{
    [SerializeField, Min(0f)] private float radius = 4f;
    [SerializeField] private NoiseType noiseType = NoiseType.ObjectImpact;
    
    [SerializeField] private LayerMask groundMask;
    
    private NoiseSystem _noiseSystem;

    void Start()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundMask.value & (1 << collision.gameObject.layer)) == 0)
            return;
            
        Vector3 position = transform.position;
        if (collision.contactCount > 0)
        {
            position = collision.GetContact(0).point;
        }

        // Emit noise on impact (e.g., stone hits ground).
        _noiseSystem.EmitNoise(position, radius, gameObject, noiseType);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((groundMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        Vector3 position = other.ClosestPoint(transform.position);

        // Emit noise on trigger impact (e.g., projectile with trigger collider).
        _noiseSystem.EmitNoise(position, radius, gameObject, noiseType);
    }
}

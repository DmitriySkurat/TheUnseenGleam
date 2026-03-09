using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NoiseOnImpact : MonoBehaviour
{
    [SerializeField, Min(0f)] private float radius = 4f;
    [SerializeField] private NoiseType noiseType = NoiseType.ObjectImpact;
    
    private NoiseSystem _noiseSystem;

    void Start()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector3 position = transform.position;
        if (collision.contactCount > 0)
        {
            position = collision.GetContact(0).point;
        }

        // Emit noise on impact (e.g., stone hits ground).
        _noiseSystem.EmitNoise(position, radius, gameObject, noiseType);
    }
}

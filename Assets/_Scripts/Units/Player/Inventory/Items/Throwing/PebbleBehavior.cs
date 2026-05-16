using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PebbleBehavior : MonoBehaviour
{
    public float pebbleSpeed = 5f;
    public float pebbleGravity = 3f;

    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private LayerMask whatDestroysPebble;
    [SerializeField] private NoiseType impactNoiseType = NoiseType.ObjectImpact;
    [SerializeField] private int maxBounces = 5;
    [SerializeField, Range(0f, 1f)] private float bounciness = 0.65f;

    private float impactNoiseRadius;
    private float _noiseVariance;
    private Rigidbody2D _rb;
    private NoiseSystem _noiseSystem;
    private int _bouncesLeft;
    private bool _bouncedThisFrame;
    private readonly HashSet<int> _ignoredColliders = new HashSet<int>();

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _noiseSystem = Services.Get<NoiseSystem>();
        _bouncesLeft = maxBounces;

        if (!Services.IsRegistered<PlayerContext>())
            return;
        var playerCtx = Services.Get<PlayerContext>();
        impactNoiseRadius = playerCtx.noiseStats.PebbleImpactNoiseRadius;
        _noiseVariance = playerCtx.noiseStats.RadiusVariance;

        Destroy(gameObject, destroyTime);
        InitializeBulletStats();
    }

    private void FixedUpdate()
    {
        transform.up = _rb.linearVelocity;
        _bouncedThisFrame = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_bouncedThisFrame) return;
        if (_ignoredColliders.Contains(collision.GetInstanceID())) return;

        if (collision.TryGetComponent(out IRockActivatable activatable))
        {
            Vector2 point = collision.ClosestPoint(transform.position);
            activatable.OnHitByRock(point, gameObject);
            Bounce(collision);
            return;
        }

        if (collision.TryGetComponent(out BreakableVase vase))
        {
            Vector2 point = collision.ClosestPoint(transform.position);
            vase.OnHitByProjectile(point, gameObject);
            Bounce(collision);
            return;
        }

        if ((whatDestroysPebble.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (impactNoiseRadius > 0f)
            {
                Vector2 point = collision.ClosestPoint(transform.position);
                _noiseSystem.EmitNoise(point, impactNoiseRadius, gameObject, impactNoiseType, _noiseVariance);
            }

            Bounce(collision);
        }
    }

    private void Bounce(Collider2D collision)
    {
        _bouncesLeft--;
        if (_bouncesLeft <= 0)
        {
            Destroy(gameObject);
            return;
        }

        _bouncedThisFrame = true;

        Vector2 velocity = _rb.linearVelocity;
        Vector2 normal = GetSurfaceNormal(velocity.normalized, collision);
        Vector2 reflected = Vector2.Reflect(velocity, normal) * bounciness;

        _rb.linearVelocity = reflected;
        // Выталкиваем камень за пределы поверхности, чтобы триггер не сработал повторно
        _rb.position += normal * 0.12f;

        int id = collision.GetInstanceID();
        _ignoredColliders.Add(id);
        StartCoroutine(RemoveIgnore(id));
    }

    private IEnumerator RemoveIgnore(int id)
    {
        yield return new WaitForSeconds(0.15f);
        _ignoredColliders.Remove(id);
    }

    // Raycast немного позади камня в направлении движения, чтобы получить нормаль поверхности.
    private Vector2 GetSurfaceNormal(Vector2 direction, Collider2D col)
    {
        Vector2 origin = (Vector2)transform.position - direction * 0.3f;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.6f);
        if (hit.collider == col)
            return hit.normal;

        Vector2 toSurface = col.ClosestPoint(transform.position) - (Vector2)transform.position;
        if (toSurface.sqrMagnitude > 0.001f)
            return -toSurface.normalized;

        return -direction;
    }

    private void InitializeBulletStats()
    {
        _rb.linearVelocity = (Vector2)transform.up * pebbleSpeed;
        _rb.gravityScale = pebbleGravity;
    }

    public void SetThrowStats(float speed, float gravity)
    {
        pebbleSpeed = speed;
        pebbleGravity = gravity;

        if (_rb == null) return;

        _rb.linearVelocity = (Vector2)transform.up * pebbleSpeed;
        _rb.gravityScale = pebbleGravity;
    }
}

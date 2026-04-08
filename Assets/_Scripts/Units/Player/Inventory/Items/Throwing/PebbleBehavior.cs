using UnityEngine;

public class PebbleBehavior : MonoBehaviour
{
    public float pebbleSpeed = 5f;
    public float pebbleGravity = 3f;
    
    [SerializeField] private float destroyTime = 3f;
    [SerializeField] private LayerMask whatDestroysPebble;
    [SerializeField] private NoiseType impactNoiseType = NoiseType.ObjectImpact;

    private float impactNoiseRadius;
    private Rigidbody2D _rb;
    private NoiseSystem _noiseSystem;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _noiseSystem = Services.Get<NoiseSystem>();
        var playerCtx = Services.Get<PlayerContext>();
        impactNoiseRadius = playerCtx.noiseStats.PebbleImpactNoiseRadius;
        
        SetDestroyTime();
        
        InitializeBulletStats();
    }
    
    private void FixedUpdate()
    {
       // rotate pebble
       
       transform.up = _rb.linearVelocity;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if((whatDestroysPebble.value & (1 << collision.gameObject.layer)) > 0)
        {
            // spawn particles
            // play fx
            if (impactNoiseRadius > 0f)
            {
                Vector2 point = collision.ClosestPoint(transform.position);
                _noiseSystem.EmitNoise(point, impactNoiseRadius, gameObject, impactNoiseType);
            }
            
            Destroy(gameObject);
        }
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
    
    private void SetDestroyTime() 
    {
        Destroy(gameObject, destroyTime);
    }
}

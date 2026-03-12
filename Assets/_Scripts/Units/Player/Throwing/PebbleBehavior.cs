using UnityEngine;

public class PebbleBehavior : MonoBehaviour
{
    public float pebbleSpeed = 5f;
    public float pebbleGravity = 3f;
    
    [SerializeField] private float destroyTime = 3f;
    [SerializeField] private LayerMask whatDestroysPebble;
    
    
    private Rigidbody2D _rb;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
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

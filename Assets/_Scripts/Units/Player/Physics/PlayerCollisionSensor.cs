using UnityEngine;

public class PlayerCollisionSensor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player;
    
    private PlayerContext _ctx;
    
    private CapsuleCollider2D _col;
    private bool _cachedQueryStartInColliders;
    
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        
        _col = GetComponent<CapsuleCollider2D>();
        _ctx.coll = _col;
        
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    public void FixedUpdate()
    {
        if (_ctx == null || _col == null) return;
            
        CheckCollisions();
    }

    void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        bool wasGrounded = _ctx.grounded;
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.down, _ctx.stats.GrounderDistance, ~_ctx.stats.GroundLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.GrounderDistance, ~_ctx.stats.GroundLayer);

        if (ceilingHit) _ctx.velocity.y = Mathf.Min(0, _ctx.velocity.y);

        if (!wasGrounded && groundHit) {
            _ctx.grounded = true;
            _ctx.coyoteUsable = true;
            _ctx.bufferedJumpUsable = true;
            _ctx.endedJumpEarly = false;
        } else if (wasGrounded && !groundHit) {
            _ctx.grounded = false;
            _ctx.frameLeftGrounded = Time.time;
        } else {
            _ctx.grounded = groundHit;
        }
        
        if (_ctx.isCrouching)
        {
            _ctx.ceilingAbove = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.CeilingCheckDistance, ~_ctx.stats.GroundLayer);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Ladder>())
        {
            _ctx.onLadder = true;
        }
        
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Ladder>())
        {
            _ctx.onLadder = false;
        }

       
    }
}

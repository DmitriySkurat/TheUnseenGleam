using UnityEngine;

public class PlayerMovementMotor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player;
    
    private PlayerContext _ctx;
    private Rigidbody2D _rb;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _rb = GetComponent<Rigidbody2D>();
        
        _ctx.velocity = _rb.linearVelocity;
    }

    void FixedUpdate()
    {
        if (_ctx == null || _rb == null) return;
        
        UpdateMovementGrace(Time.fixedDeltaTime);
        HandleGravity(Time.fixedDeltaTime);
        ApplyMovement();
    }
    
    void HandleGravity(float deltaTime) 
    {
        if (_ctx.isClimbing)
        {
            if (_ctx.onLian)
            {
                float target = -_ctx.stats.LianSlipSpeed;
                _ctx.velocity.y = Mathf.MoveTowards(_ctx.velocity.y, target, _ctx.stats.LianSlipAcceleration * deltaTime);
            }
            return;
        }

        if (_ctx.grounded && _ctx.velocity.y <= 0f) {
            _ctx.velocity.y = _ctx.stats.GroundingForce;
        } else {
            var inAirGravity = _ctx.stats.FallAcceleration;
            if (_ctx.endedJumpEarly && _ctx.velocity.y > 0) inAirGravity *= _ctx.stats.JumpEndEarlyGravityModifier;
            _ctx.velocity.y = Mathf.MoveTowards(_ctx.velocity.y, -_ctx.stats.MaxFallSpeed, inAirGravity * deltaTime);
        }
    }
    
    
    void UpdateMovementGrace(float deltaTime)
    {
        if (Mathf.Abs(_ctx.input.Move.x) > 0.01f)
        {
            _ctx.movementGraceTimer = _ctx.stats.GraceTime;
        }
        else
        {
            _ctx.movementGraceTimer -= deltaTime;
        }
    }

    void ApplyMovement()
    {
        _rb.linearVelocity = _ctx.velocity; 
    } 
}

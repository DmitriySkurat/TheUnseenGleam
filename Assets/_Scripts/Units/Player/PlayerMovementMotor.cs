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
        ApplyMovement();
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

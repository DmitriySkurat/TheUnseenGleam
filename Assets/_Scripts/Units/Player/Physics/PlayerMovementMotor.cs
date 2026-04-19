using UnityEngine;

public class PlayerMovementMotor : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;
    
    private PlayerContext _ctx;
    private Rigidbody2D _rb;
    private float _lastAppliedVelocityY;
    private float _lastAppliedVelocityX;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();
        _rb = GetComponent<Rigidbody2D>();
        
        _ctx.velocity = _rb.linearVelocity;
    }
    
    public void Dispose()
    {
        
    }

    void FixedUpdate()
    {
        if (_ctx == null || _rb == null) return;

        // Если в прошлом кадре мы применили положительную скорость, но физика её обнулила (удар о потолок) — синхронизируемся
        if (_lastAppliedVelocityY > 0 && _rb.linearVelocity.y <= 0)
            _ctx.velocity.y = _rb.linearVelocity.y;

        // Если физика остановила горизонтальное движение (удар о стену) — синхронизируемся
        if (_lastAppliedVelocityX > 0 && _rb.linearVelocity.x <= 0 ||
            _lastAppliedVelocityX < 0 && _rb.linearVelocity.x >= 0)
            _ctx.velocity.x = _rb.linearVelocity.x;

        UpdateMovementGrace(Time.fixedDeltaTime);
        HandleGravity(Time.fixedDeltaTime);
        ApplyMovement();
        _lastAppliedVelocityY = _ctx.velocity.y;
        _lastAppliedVelocityX = _ctx.velocity.x;
    }
    
    void HandleGravity(float deltaTime)
    {
        if (_ctx.isGrabbed)
        {
            _ctx.velocity.y = 0f;
            return;
        }

        if (_ctx.isLedgeGrabbing)
        {
            _ctx.velocity.y = 0f;
            return;
        }

        if (_ctx.isClimbing)
        {
            if (_ctx.onVines)
            {
                float target = -_ctx.stats.VinesSlipSpeed;
                _ctx.velocity.y = Mathf.MoveTowards(_ctx.velocity.y, target, _ctx.stats.VinesSlipAcceleration * deltaTime);
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
        ClampVelocityAgainstWalls();
        _rb.linearVelocity = _ctx.velocity;
    }

    void ClampVelocityAgainstWalls()
    {
        if (_ctx.velocity.x == 0f || _ctx.coll == null || _ctx.stats == null) return;
        if (!(_ctx.coll is CapsuleCollider2D col)) return;

        bool cachedQuery = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;

        Vector2 dir = _ctx.velocity.x > 0f ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0f, dir, _ctx.stats.GrounderDistance, _ctx.stats.GroundLayer);

        Physics2D.queriesStartInColliders = cachedQuery;

        // Только настоящие стены имеют преимущественно горизонтальную нормаль.
        // Угловые грани тайлов пола имеют вертикальную нормаль — их не блокируем.
        // Гасим горизонтальную скорость либо при нормали стены, либо при обнаруженном уступе —
        // иначе скруглённый конец капсулы «объедет» угол раньше, чем LedgeClimb успеет сработать.
        if (hit && (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.y) || _ctx.canGrabLedge))
            _ctx.velocity.x = 0f;
    }
}

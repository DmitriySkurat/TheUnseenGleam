using UnityEngine;

public class PlayerCollisionSensor : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;
    
    private PlayerContext _ctx;
    private NoiseSystem _noiseSystem;
    
    private CapsuleCollider2D _col;
    private bool _cachedQueryStartInColliders;
    
    private bool _ceilingHit;
    private bool _groundHit;
    
    
    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();
        _noiseSystem = Services.Get<NoiseSystem>();
        
        _col = GetComponent<CapsuleCollider2D>();
        _ctx.coll = _col;
        _ctx.airborneStartY = transform.position.y;
        
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }
    
    public void Dispose()
    {
        
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
        float currentY = _ctx.transform != null ? _ctx.transform.position.y : transform.position.y;

        _groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.down, _ctx.stats.GrounderDistance, _ctx.stats.GroundLayer);
        _ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.GrounderDistance, _ctx.stats.GroundLayer);

        if (_ceilingHit) _ctx.velocity.y = Mathf.Min(0, _ctx.velocity.y);

        if (!wasGrounded && _groundHit) {
            _ctx.grounded = true;
            _ctx.coyoteUsable = true;
            _ctx.bufferedJumpUsable = true;
            _ctx.endedJumpEarly = false;
            HandleLanding(currentY);
        } else if (wasGrounded && !_groundHit) {
            _ctx.grounded = false;
            _ctx.frameLeftGrounded = Time.time;
            _ctx.airborneStartY = currentY;
            _ctx.landingRollEndTime = float.MinValue;
            _ctx.landingRollDirection = 0f;
        } else {
            _ctx.grounded = _groundHit;
            if (_groundHit)
            {
                _ctx.airborneStartY = currentY;
            }
        }
        
        if (_ctx.isCrouching)
        {
            _ctx.ceilingAbove = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.CeilingCheckDistance, _ctx.stats.GroundLayer);
        }

        CheckLedgeGrab();

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    void HandleLanding(float landingY)
    {
        float fallHeight = Mathf.Max(0f, _ctx.airborneStartY - landingY);
        _ctx.lastFallHeight = fallHeight;

        bool didRoll = ShouldStartLandingRoll(fallHeight);
        _ctx.landingRollEndTime = didRoll
            ? Time.time + _ctx.stats.LandingRollDuration
            : float.MinValue;
        _ctx.landingRollDirection = didRoll ? Mathf.Sign(_ctx.velocity.x != 0f ? _ctx.velocity.x : _ctx.input.Move.x) : 0f;

        EmitLandingNoise(fallHeight, didRoll);
        _ctx.airborneStartY = landingY;
    }

    bool ShouldStartLandingRoll(float fallHeight)
    {
        if (_ctx.stats == null) return false;
        if (fallHeight < _ctx.stats.LandingRollMinFallHeight) return false;
        if (!_ctx.input.CrouchHeld && !_ctx.HasLandingRollBuffered) return false;
        if (Mathf.Abs(_ctx.velocity.x) < _ctx.stats.LandingRollMinHorizontalSpeed) return false;

        return true;
    }

    void EmitLandingNoise(float fallHeight, bool didRoll)
    {
        if (_noiseSystem == null || _ctx.noiseStats == null || _ctx.transform == null) return;

        float radius = EvaluateLandingNoiseRadius(fallHeight);
        if (didRoll)
        {
            radius *= _ctx.noiseStats.LandingRollNoiseMultiplier;
        }

        _noiseSystem.EmitNoise(_ctx.transform.position, radius, _ctx.transform.gameObject, NoiseType.Landing);
    }

    float EvaluateLandingNoiseRadius(float fallHeight)
    {
        if (_ctx.noiseStats == null) return 0f;
        if (fallHeight < _ctx.noiseStats.LandingNoiseMinFallHeight) return 0f;

        float maxHeight = Mathf.Max(_ctx.noiseStats.LandingNoiseMinFallHeight + 0.01f, _ctx.noiseStats.LandingNoiseMaxFallHeight);
        float t = Mathf.InverseLerp(_ctx.noiseStats.LandingNoiseMinFallHeight, maxHeight, fallHeight);
        return Mathf.Lerp(_ctx.noiseStats.LandingNoiseMinRadius, _ctx.noiseStats.LandingNoiseMaxRadius, t);
    }
    
    void CheckLedgeGrab()
    {
        if (_ctx.grounded || _ctx.isLedgeGrabbing || _ctx.isClimbing || _ctx.stats == null)
        {
            _ctx.canGrabLedge = false;
            return;
        }

        float dirInput = _ctx.input.Move.x;
        float dirVel   = _ctx.velocity.x;
        float dir = 0f;
        if (Mathf.Abs(dirInput) > _ctx.stats.HorizontalDeadZoneThreshold)
            dir = Mathf.Sign(dirInput);
        else if (Mathf.Abs(dirVel) > 0.1f)
            dir = Mathf.Sign(dirVel);

        if (dir == 0f)
        {
            _ctx.canGrabLedge = false;
            return;
        }

        bool cachedQuery = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;

        float   checkY     = _col.bounds.min.y + _ctx.stats.LedgeWallCheckHeight;
        float   topCheckY  = checkY + _ctx.stats.LedgeTopCheckOffset;
        float   rayLen     = _col.bounds.extents.x + _ctx.stats.LedgeCheckDistance;
        Vector2 horizontal = new Vector2(dir, 0f);

        // Нижний луч попадает → стена есть на уровне захвата.
        RaycastHit2D wallHit = Physics2D.Raycast(new Vector2(_col.bounds.center.x, checkY),    horizontal, rayLen, _ctx.stats.GroundLayer);
        // Верхний луч НЕ попадает → над захватом свободно, значит это уступ, а не сплошная стена.
        RaycastHit2D topHit  = Physics2D.Raycast(new Vector2(_col.bounds.center.x, topCheckY), horizontal, rayLen, _ctx.stats.GroundLayer);

        // Вертикальный луч сверху вниз — находит реальную Y-координату поверхности уступа,
        // независимо от текущей высоты игрока.
        Vector2 downOrigin = new Vector2(wallHit.point.x + dir * 0.05f, topCheckY);
        RaycastHit2D downHit = Physics2D.Raycast(downOrigin, Vector2.down,
            _ctx.stats.LedgeTopCheckOffset + 0.5f, _ctx.stats.GroundLayer);

        Physics2D.queriesStartInColliders = cachedQuery;

        if (!wallHit || topHit || !downHit)
        {
            _ctx.canGrabLedge = false;
            return;
        }

        // Проверяем, хватает ли места для коллайдера над уступом (иначе не сможем встать).
        // Используем OverlapCapsule с queriesStartInColliders=true, чтобы поймать потолок даже
        // когда он вплотную прилегает к поверхности уступа (raycast стартовал бы внутри тайла).
        float halfH = _col.size.y * 0.5f;
        Vector2 standCenter = new Vector2(
            wallHit.point.x + dir * (_col.size.x * 0.5f + _ctx.stats.LedgeStandOffsetX),
            downHit.point.y + halfH + _ctx.stats.LedgeStandOffsetY
        );

        Physics2D.queriesStartInColliders = true;
        Collider2D standOverlap = Physics2D.OverlapCapsule(standCenter, _col.size, _col.direction, 0f, _ctx.stats.GroundLayer);
        Physics2D.queriesStartInColliders = false;

        if (standOverlap != null)
        {
            _ctx.canGrabLedge = false;
            return;
        }

        _ctx.canGrabLedge        = true;
        _ctx.ledgeFacingRight    = dir > 0f;
        _ctx.ledgeCornerPosition = new Vector2(wallHit.point.x, downHit.point.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ladder = other.GetComponent<Ladder>();
        var vines = other.GetComponent<Vines>();

        if (ladder != null || vines != null)
        {
            _ctx.onLadder = true;
        }
        
        if (vines != null)
        {
            _ctx.onVines = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var ladder = other.GetComponent<Ladder>();
        var vines = other.GetComponent<Vines>();

        if (ladder != null || vines != null)
        {
            _ctx.onLadder = false;
        }
        
        if (vines != null)
        {
            _ctx.onVines = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        var col = _ctx.coll as CapsuleCollider2D;
        
        // Коллайдер
        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.offset, col.size);

        Gizmos.matrix = Matrix4x4.identity;
        
        // CeilingAbove
        Gizmos.color = _ctx.ceilingAbove ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _ctx.stats.CeilingCheckDistance);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _ctx.stats.CeilingCheckDistance, 0.05f);

        // Ledge detection rays
        if (_ctx.stats != null)
        {
            float checkY    = col.bounds.min.y + _ctx.stats.LedgeWallCheckHeight;
            float topCheckY = checkY + _ctx.stats.LedgeTopCheckOffset;
            float rayLength = col.bounds.extents.x + _ctx.stats.LedgeCheckDistance;
            Gizmos.color = _ctx.canGrabLedge ? Color.cyan : new Color(0f, 1f, 1f, 0.25f);
            // нижний — стена должна быть
            Gizmos.DrawLine(new Vector3(col.bounds.center.x - rayLength, checkY,    0f),
                            new Vector3(col.bounds.center.x + rayLength, checkY,    0f));
            // верхний — стены быть не должно
            Gizmos.DrawLine(new Vector3(col.bounds.center.x - rayLength, topCheckY, 0f),
                            new Vector3(col.bounds.center.x + rayLength, topCheckY, 0f));
            if (_ctx.canGrabLedge)
            {
                Gizmos.DrawWireSphere(_ctx.ledgeCornerPosition, 0.08f);
                float dir2 = _ctx.ledgeFacingRight ? 1f : -1f;
                float halfH2 = col.size.y * 0.5f;
                Vector3 standCenter2 = new Vector3(
                    _ctx.ledgeCornerPosition.x + dir2 * (col.size.x * 0.5f + _ctx.stats.LedgeStandOffsetX),
                    _ctx.ledgeCornerPosition.y + halfH2 + _ctx.stats.LedgeStandOffsetY,
                    0f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(standCenter2, new Vector3(col.size.x, col.size.y, 0f));
            }
        }

        // Проверка вниз (groundHit)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(col.bounds.center, col.bounds.center + Vector3.down * _ctx.stats.GrounderDistance);
        Gizmos.DrawWireSphere(col.bounds.center + Vector3.down * _ctx.stats.GrounderDistance, 0.05f);

        // Проверка вверх (ceilingHit)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(col.bounds.center, col.bounds.center + Vector3.up * _ctx.stats.GrounderDistance);
        Gizmos.DrawWireSphere(col.bounds.center + Vector3.up * _ctx.stats.GrounderDistance, 0.05f);
    
    }
}

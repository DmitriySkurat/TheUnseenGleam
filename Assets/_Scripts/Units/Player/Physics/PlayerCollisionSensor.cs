using UnityEngine;

public class PlayerCollisionSensor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player;
    
    private PlayerContext _ctx;
    private NoiseSystem _noiseSystem;
    
    private CapsuleCollider2D _col;
    private bool _cachedQueryStartInColliders;
    
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _noiseSystem = Services.Get<NoiseSystem>();
        
        _col = GetComponent<CapsuleCollider2D>();
        _ctx.coll = _col;
        _ctx.airborneStartY = transform.position.y;
        
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
        float currentY = _ctx.transform != null ? _ctx.transform.position.y : transform.position.y;
        // bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.down, _ctx.stats.GrounderDistance, ~_ctx.stats.GroundLayer);
        // bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.GrounderDistance, ~_ctx.stats.GroundLayer);

        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.down, _ctx.stats.GrounderDistance, _ctx.stats.GroundLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.GrounderDistance, _ctx.stats.GroundLayer);

        if (ceilingHit) _ctx.velocity.y = Mathf.Min(0, _ctx.velocity.y);

        if (!wasGrounded && groundHit) {
            _ctx.grounded = true;
            _ctx.coyoteUsable = true;
            _ctx.bufferedJumpUsable = true;
            _ctx.endedJumpEarly = false;
            HandleLanding(currentY);
        } else if (wasGrounded && !groundHit) {
            _ctx.grounded = false;
            _ctx.frameLeftGrounded = Time.time;
            _ctx.airborneStartY = currentY;
            _ctx.landingRollEndTime = float.MinValue;
            _ctx.landingRollDirection = 0f;
        } else {
            _ctx.grounded = groundHit;
            if (groundHit)
            {
                _ctx.airborneStartY = currentY;
            }
        }
        
        if (_ctx.isCrouching)
        {
            _ctx.ceilingAbove = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _ctx.stats.CeilingCheckDistance, ~_ctx.stats.GroundLayer);
        }

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
        if (_noiseSystem == null || _ctx.stats == null || _ctx.transform == null) return;

        float radius = EvaluateLandingNoiseRadius(fallHeight);
        if (didRoll)
        {
            radius *= _ctx.stats.LandingRollNoiseMultiplier;
        }

        _noiseSystem.EmitNoise(_ctx.transform.position, radius, _ctx.transform.gameObject, NoiseType.Landing);
    }

    float EvaluateLandingNoiseRadius(float fallHeight)
    {
        if (_ctx.stats == null) return 0f;
        if (fallHeight < _ctx.stats.LandingNoiseMinFallHeight) return 0f;

        float maxHeight = Mathf.Max(_ctx.stats.LandingNoiseMinFallHeight + 0.01f, _ctx.stats.LandingNoiseMaxFallHeight);
        float t = Mathf.InverseLerp(_ctx.stats.LandingNoiseMinFallHeight, maxHeight, fallHeight);
        return Mathf.Lerp(_ctx.stats.LandingNoiseMinRadius, _ctx.stats.LandingNoiseMaxRadius, t);
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
}

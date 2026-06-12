using UnityEngine;

public class FallDamage : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 20;

    private PlayerContext _ctx;
    private bool _wasGrounded;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;

        _ctx = Services.Get<PlayerContext>();
    }

    public void Dispose()
    {
    }

    private void Update()
    {
        if (_ctx == null)
            return;

        bool grounded = _ctx.grounded;

        if (!_wasGrounded && grounded)
        {
            ApplyFallDamage();
        }

        _wasGrounded = grounded;
    }

    private void ApplyFallDamage()
    {
        if (_ctx == null || _ctx.stats == null)
            return;

        float fallHeight = _ctx.lastFallHeight;

        if (fallHeight < _ctx.stats.FallDamageMinHeight)
            return;

        bool didPerformRoll = _ctx.lastLandingWasRoll;

        if (fallHeight >= _ctx.stats.FallDamageLethalHeight)
        {
            _ctx.health.TakeDamage(9999f);
            return;
        }

        float damage =
            (fallHeight - _ctx.stats.FallDamageMinHeight)
            * _ctx.stats.FallDamagePerUnit;

        if (didPerformRoll)
        {
            damage *= _ctx.stats.FallDamageRollMultiplier;
        }

        _ctx.health.TakeDamage(damage);
    }
}
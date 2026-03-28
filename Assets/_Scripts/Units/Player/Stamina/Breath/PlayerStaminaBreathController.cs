using UnityEngine;

public class PlayerStaminaBreathController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 5;

    private PlayerContext _ctx;
    private NoiseSystem _noiseSystem;
    private float _breathingNoiseTimer;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _noiseSystem = Services.Get<NoiseSystem>();

        if (_ctx == null || _ctx.stats == null)
            return;

        _ctx.isHoldingBreath = false;
        _ctx.currentStaminaBreathDrainMultiplier = 0f;
        _breathingNoiseTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (_ctx == null || _ctx.stats == null || _ctx.transform == null || !_ctx.isAlive)
        {
            _breathingNoiseTimer = 0f;
            return;
        }

        UpdateBreath(Time.deltaTime);
        EmitBreathingNoise(Time.deltaTime);
    }

    private void UpdateBreath(float deltaTime)
    {
        _ctx.isHoldingBreath = _ctx.input.HoldBreathHeld && _ctx.CanHoldBreath;

        if (_ctx.isHoldingBreath)
        {
            float breathDrain = _ctx.stats.StaminaHoldBreathDrainPerSecond * Mathf.Max(0f, _ctx.currentStaminaBreathDrainMultiplier);
            
            _ctx.stamina = Mathf.Max(0f, _ctx.stamina - breathDrain * deltaTime);
            
            if (_ctx.stamina <= 0f)
            {
                _ctx.isHoldingBreath = false;
            }
        }
    }

    private void EmitBreathingNoise(float deltaTime)
    {
        if (_noiseSystem == null
            || _ctx.isHoldingBreath
            || _ctx.stats.BreathingNoiseRadius <= 0f
            || _ctx.stats.BreathingNoiseInterval <= 0f)
        {
            _breathingNoiseTimer = 0f;
            return;
        }

        _breathingNoiseTimer += deltaTime;
        if (_breathingNoiseTimer < _ctx.stats.BreathingNoiseInterval)
            return;

        _breathingNoiseTimer = 0f;
        _noiseSystem.EmitNoise(
            _ctx.transform.position,
            _ctx.stats.BreathingNoiseRadius,
            _ctx.transform.gameObject,
            NoiseType.Breathing
        );
    }
}

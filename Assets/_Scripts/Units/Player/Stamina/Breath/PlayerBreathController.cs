using UnityEngine;

public class PlayerBreathController : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 5;

    [Header("Hide In Light")]
    [Tooltip("Минимальная сила освещённости, при которой задержка дыхания скрывает игрока от ослеплённого агента")]
    [SerializeField, Min(0f)] private float _hidingLightThreshold = 0.5f;

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
    
    public void Dispose()
    {
        
    }

    private void FixedUpdate()
    {
        if (_ctx == null || _ctx.stats == null || _ctx.transform == null || !_ctx.isAlive)
        {
            _ctx.isHoldingBreath  = false;
            _breathingNoiseTimer  = 0f;
            return;
        }

        UpdateBreath(Time.fixedDeltaTime);
        EmitBreathingNoise(Time.fixedDeltaTime);
    }

    private void UpdateBreath(float deltaTime)
    {
        if (!_ctx.CanHoldBreath)
        {
            _ctx.isHoldingBreath = false;
            return;
        }

        if (_ctx.isHoldingBreath)
        {
            if (!_ctx.input.HoldBreathHeld || _ctx.stamina <= 0f)
            {
                _ctx.isHoldingBreath = false;
                return;
            }
        }
        else if (_ctx.input.HoldBreathHeld && _ctx.stamina >= _ctx.stats.MinStaminaToHoldBreath)
        {
            _ctx.isHoldingBreath = true;
        }

        if (_ctx.isHoldingBreath)
        {
            float breathDrain = _ctx.stats.StaminaHoldBreathDrainPerSecond * Mathf.Max(0f, _ctx.currentStaminaBreathDrainMultiplier);
            _ctx.stamina = Mathf.Max(0f, _ctx.stamina - breathDrain * deltaTime);

            if (_ctx.stamina <= 0f)
                _ctx.isHoldingBreath = false;
        }
    }

    private void EmitBreathingNoise(float deltaTime)
    {
        if (_noiseSystem == null
            || _ctx.isHoldingBreath
            || _ctx.noiseStats == null
            || _ctx.noiseStats.BreathingNoiseRadius <= 0f
            || _ctx.noiseStats.BreathingNoiseInterval <= 0f)
        {
            _breathingNoiseTimer = 0f;
            return;
        }

        _breathingNoiseTimer += deltaTime;
        if (_breathingNoiseTimer < _ctx.noiseStats.BreathingNoiseInterval)
            return;

        _breathingNoiseTimer = 0f;
        _noiseSystem.EmitNoise(
            _ctx.transform.position,
            _ctx.noiseStats.BreathingNoiseRadius,
            _ctx.transform.gameObject,
            NoiseType.Breathing
        );
    }
}

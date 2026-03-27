using UnityEngine;

public class PlayerBreathController : MonoBehaviour, IInitializable
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

        _ctx.currentBreath = _ctx.stats.MaxBreathCapacity;
        _ctx.isHoldingBreath = false;
        _ctx.isOutOfBreath = false;
        _breathingNoiseTimer = 0f;
    }

    private void Update()
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
        bool wantsHoldBreath = _ctx.input.HoldBreathHeld;
        bool canHoldBreath = _ctx.currentBreath > 0f;

        _ctx.isHoldingBreath = wantsHoldBreath && canHoldBreath;

        if (_ctx.isHoldingBreath)
        {
            _ctx.currentBreath = Mathf.Max(0f, _ctx.currentBreath - _ctx.stats.BreathDrainPerSecond * deltaTime);
            if (_ctx.currentBreath <= 0f)
            {
                _ctx.currentBreath = 0f;
                _ctx.isHoldingBreath = false;
            }
        }
        else if (!wantsHoldBreath)
        {
            _ctx.currentBreath = Mathf.Min(
                _ctx.stats.MaxBreathCapacity,
                _ctx.currentBreath + _ctx.stats.BreathRecoveryPerSecond * deltaTime
            );
        }

        _ctx.isOutOfBreath = _ctx.currentBreath <= 0f;
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

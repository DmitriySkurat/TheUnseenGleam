using UnityEngine;
using UnityEngine.Audio;

public class PlayerBreathController : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 5;

    [Header("Hide In Light")]
    [Tooltip("Минимальная сила освещённости, при которой задержка дыхания скрывает игрока от ослеплённого агента")]
    [SerializeField, Min(0f)] private float _hidingLightThreshold = 0.5f;

    [Header("Audio")]
    [SerializeField] private LayerMask        _enemyMask;
    [SerializeField] private AudioMixerGroup  _sfxGroup;

    private PlayerContext _ctx;
    private NoiseSystem   _noiseSystem;
    private AudioSource   _breathingSource;
    private float _breathingNoiseTimer;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx         = Services.Get<PlayerContext>();
        _noiseSystem = Services.Get<NoiseSystem>();

        _breathingSource                    = gameObject.AddComponent<AudioSource>();
        _breathingSource.loop               = true;
        _breathingSource.playOnAwake        = false;
        _breathingSource.spatialBlend       = 0f;
        _breathingSource.outputAudioMixerGroup = _sfxGroup;

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
            
            if (_breathingSource != null && _breathingSource.isPlaying)
               _breathingSource.Stop();
        
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
            NoiseType.Breathing,
            _ctx.noiseStats.RadiusVariance
        );

        UpdateBreathingAudio();
    }

    private void UpdateBreathingAudio()
    {
        if (_breathingSource == null || _ctx.noiseStats == null) return;

        var clip = _ctx.noiseStats.BreathingClip;
        if (clip == null) return;

        float radius = _ctx.noiseStats.BreathingEnemyProximityRadius;
        bool enemyNearby = radius > 0f &&
            Physics2D.OverlapCircle(_ctx.transform.position, radius, _enemyMask) != null;

        if (enemyNearby && !_breathingSource.isPlaying)
        {
            _breathingSource.clip = clip;
            _breathingSource.Play();
        }
        else if (!enemyNearby && _breathingSource.isPlaying)
        {
            _breathingSource.Stop();
        }
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "NoiseScriptableStats", menuName = "Stats/Noise Stats")]
public class NoiseScriptableStats : ScriptableObject
{
    [Header("FOOTSTEPS")]
    [Tooltip("Minimum horizontal input required to emit footstep noise")]
    [Range(0.01f, 1f)]
    public float NoiseMoveThreshold = 0.1f;

    [Tooltip("Interval between footstep noise pulses while walking")]
    [Min(0.05f)]
    public float WalkFootstepInterval = 0.5f;

    [Tooltip("Interval between footstep noise pulses while running")]
    [Min(0.05f)]
    public float RunFootstepInterval = 0.35f;

    [Tooltip("Interval between footstep noise pulses while crouching")]
    [Min(0.05f)]
    public float CrouchFootstepInterval = 0.7f;

    [Tooltip("Noise radius emitted by each walking footstep")]
    [Min(0f)]
    public float WalkNoiseRadius = 2f;

    [Tooltip("Noise radius emitted by each running footstep")]
    [Min(0f)]
    public float RunNoiseRadius = 4f;

    [Tooltip("Noise radius emitted by each crouching footstep")]
    [Min(0f)]
    public float CrouchNoiseRadius = 1f;




    [Header("JUMP")]
    [Tooltip("Noise radius emitted at the moment the player leaves the ground")]
    [Min(0f)]
    public float JumpStartNoiseRadius = 1.5f;
    
    [Header("LANDING")]
    [Tooltip("Minimum fall height required before landing noise is emitted")]
    [Min(0f)]
    public float LandingNoiseMinFallHeight = 0.75f;

    [Tooltip("Fall height that maps to the maximum landing noise radius")]
    [Min(0.01f)]
    public float LandingNoiseMaxFallHeight = 6f;

    [Tooltip("Landing noise radius at the minimum configured fall height")]
    [Min(0f)]
    public float LandingNoiseMinRadius = 1.5f;

    [Tooltip("Landing noise radius at the maximum configured fall height")]
    [Min(0f)]
    public float LandingNoiseMaxRadius = 6f;

    [Tooltip("Multiplier applied to landing noise radius when the player rolls on landing")]
    [Range(0f, 1f)]
    public float LandingRollNoiseMultiplier = 0.45f;
    
    

    [Header("BREATHING")]
    [Tooltip("Interval between passive breathing noise pulses")]
    [Min(0.05f)]
    public float BreathingNoiseInterval = 0.75f;

    [Tooltip("Noise radius emitted by normal breathing")]
    [Min(0f)]
    public float BreathingNoiseRadius = 1.2f;

    

    [Header("PEBBLE")]
    [Tooltip("Noise radius emitted when a pebble hits a surface")]
    [Min(0f)]
    public float PebbleImpactNoiseRadius = 4f;
}

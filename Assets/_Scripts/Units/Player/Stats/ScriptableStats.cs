using UnityEngine;

[CreateAssetMenu]
public class PlayerScriptableStats : ScriptableObject
{
    [Header("LAYERS")]
    [Tooltip("Set this to the layer your player is on")]
    public LayerMask PlayerLayer;
    
    [Tooltip("Layer considered as ground")]
    public LayerMask GroundLayer;

    [Header("INPUT")]
    [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
    public bool SnapInput = true;

    [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f;

    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;

    [Header("MOVEMENT")]
    [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;

    [Tooltip("The player's capacity to gain horizontal speed")]
    public float Acceleration = 120;

    [Tooltip("The pace at which the player comes to a stop")]
    public float GroundDeceleration = 60;

    [Tooltip("Deceleration in air only after stopping input mid-air")]
    public float AirDeceleration = 30;

    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;

    [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;
    
    [Tooltip("Multiplier applied to MaxSpeed while walking")]
    public float WalkSpeedMultiplier = 1f;
    
    [Tooltip("Multiplier applied to MaxSpeed while running")]
    [Range(1f, 2f)]
    public float RunSpeedMultiplier = 1.5f;
    
    [Tooltip("Window of time during which a specific input is still considered valid, even if the timing wasn't frame-perfect")]  
    [Range(0.01f, 0.5f)]
    public float GraceTime = 0.08f;
    
    [Header("STAMINA")]
    [Tooltip("Maximum stamina value")]
    public float MaxStamina = 100f;

    [Tooltip("Stamina drained per second while running")]
    public float StaminaDrainPerSecond = 20f;

    [Tooltip("Stamina regenerated per second when not running")]
    public float StaminaRegenPerSecond = 15f;
    
    [Tooltip("Minimum stamina required to START running")]
    public float MinStaminaToRun = 35f;

    [Header("JUMP")]
    [Tooltip("The immediate velocity applied when jumping")]
    public float JumpPower = 36;

    [Tooltip("The maximum vertical movement speed")]
    public float MaxFallSpeed = 40;

    [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
    public float FallAcceleration = 110;

    [Tooltip("The gravity multiplier added when jump is released early")]
    public float JumpEndEarlyGravityModifier = 3;

    [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
    public float CoyoteTime = .15f;

    [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
    public float JumpBuffer = .2f;
    
    [Header("INTERACTION")]
    [Tooltip("Delay between interactions")]
    [Range(0f, 0.5f)]
    public float InteractionCooldown = 0.25f;
    
    [Header("CROUCH")]
    [Tooltip("Multiplier applied to MaxSpeed while crouching")]
    [Range(0.1f, 1f)]
    public float CrouchSpeedMultiplier = 0.5f;

    [Tooltip("Height multiplier for CapsuleCollider while crouching")]
    [Range(0.2f, 1f)]
    public float CrouchHeightMultiplier = 0.5f;
    
    [Tooltip("Distance to check above the standing height for obstacles")]
    [Range(0f, 1f)]
    public float CeilingCheckDistance = 0.05f;

    [Tooltip("Noise multiplier while crouching (for stealth systems)")]
    [Range(0f, 1f)]
    public float CrouchNoiseMultiplier = 0.3f;

    [Header("HIDE")]
    [Tooltip("Multiplier applied to MaxSpeed while hiding")]
    [Range(0.1f, 1f)]
    public float HideSpeedMultiplier = 0.4f;

    [Header("NOISE")]
    [Tooltip("Minimum horizontal input required to emit footsteps")]
    [Range(0.01f, 1f)]
    public float NoiseMoveThreshold = 0.1f;

    [Tooltip("Footstep interval while walking")]
    [Min(0.05f)]
    public float WalkFootstepInterval = 0.5f;

    [Tooltip("Footstep interval while running")]
    [Min(0.05f)]
    public float RunFootstepInterval = 0.35f;

    [Tooltip("Footstep interval while crouching")]
    [Min(0.05f)]
    public float CrouchFootstepInterval = 0.7f;

    [Tooltip("Noise radius for walking footsteps")]
    [Min(0f)]
    public float WalkNoiseRadius = 2f;

    [Tooltip("Noise radius for running footsteps")]
    [Min(0f)]
    public float RunNoiseRadius = 4f;

    [Tooltip("Noise radius for crouch footsteps")]
    [Min(0f)]
    public float CrouchNoiseRadius = 1f;

    [Tooltip("Noise radius for jumping")]
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

    [Tooltip("How long before touching the ground a crouch input can still trigger a landing roll")]
    [Range(0f, 0.5f)]
    public float LandingRollBuffer = 0.2f;

    [Tooltip("Minimum fall height required to convert a landing into a roll")]
    [Min(0f)]
    public float LandingRollMinFallHeight = 1.5f;

    [Tooltip("Minimum horizontal speed required for a landing roll to start")]
    [Min(0f)]
    public float LandingRollMinHorizontalSpeed = 8f;

    [Tooltip("How long horizontal momentum is preserved during a landing roll")]
    [Range(0.05f, 1f)]
    public float LandingRollDuration = 0.35f;

    [Tooltip("Horizontal deceleration applied while the landing roll is active")]
    [Min(0f)]
    public float LandingRollDeceleration = 45f;

    [Tooltip("Multiplier applied to landing noise radius when the player rolls on landing")]
    [Range(0f, 1f)]
    public float LandingRollNoiseMultiplier = 0.45f;
    
    [Header("CLIMB")]
    [Tooltip("Multiplier applied to (vertical) MaxSpeed while climbing")]
    [Range(0f, 2f)]
    public float ClimbVerticalSpeedMultiplier = 0.5f;
    
    [Tooltip("Multiplier applied to (horizontal) MaxSpeed while climbing")]
    [Range(0f, 2f)]
    public float ClimbHorizontalSpeedMultiplier = 0.5f;
    
    // [Header("SLIDE")]
    // [Tooltip("Duration of the slide movement in seconds")]
    // public float SlideDuration = 2f;
    
    // [Tooltip("Multiplier applied to MaxSpeed while sliding")]
    // public float SlideSpeedMultiplier = 1.8f;
    
    // [Tooltip("Time window after running during which slide input is accepted")]
    // [Range(0f, 0.5f)]
    // public float SlideInputWindow = 3f;
    
    // [Tooltip("Height multiplier for CapsuleCollider while sliding")]
    // [Range(0.2f, 1f)]
    // public float SlideHeightMultiplier = 0.3f;
}

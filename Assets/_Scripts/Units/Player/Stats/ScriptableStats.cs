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

    [Header("HEALTH")]
    [Tooltip("Max player Health")]
    [Min(0)]
    public int MaxPlayerHealth = 100;

    [Tooltip("HP regenerated per second")]
    [Min(0f)]
    public float HealthRegenPerSecond = 2f;

    [Tooltip("Delay in seconds after taking damage before regen starts")]
    [Min(0f)]
    public float HealthRegenDelay = 5f;

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

    [Tooltip("Multiplier applied to StaminaRegenPerSecond while moving")]
    [Range(0f, 1f)]
    public float StaminaRegenMovingMultiplier = 0.4f;

    [Tooltip("Minimum stamina required to START running")]
    public float MinStaminaToRun = 35f;

    [Tooltip("Drain multiplier while running")]
    public float RunStaminaDrainMultiplier = 1f;

    [Tooltip("Drain multiplier while grabbed")]
    public float GrabbedStaminaDrainMultiplier = 1f;

    [Tooltip("Stamina cost per jump")]
    public float JumpStaminaCost = 10f;

    [Header("BREATHE (STAMINA)")]
    [Tooltip("Stamina drained per second while not breathing")]
    public float StaminaHoldBreathDrainPerSecond = 20f;

    [Tooltip("Minimum stamina required to START holding breath")]
    public float MinStaminaToHoldBreath = 25f;

    [Tooltip("Breath drain multiplier while idle")]
    [Min(0f)]
    public float IdleStaminaBreathDrainMultiplier = 1f;

    [Tooltip("Breath drain multiplier while crouching")]
    [Min(0f)]
    public float CrouchStaminaBreathDrainMultiplier = 0.8f;

    [Tooltip("Breath drain multiplier while hiding")]
    [Min(0f)]
    public float HideStaminaBreathDrainMultiplier = 0.5f;

    [Tooltip("Breath drain multiplier while pressed to wall")]
    [Min(0f)]
    public float PressToWallStaminaBreathDrainMultiplier = 1f;

    [Header("JUMP")]
    [Tooltip("Sound played at the moment of jump (stays at jump position)")]
    public AudioClip JumpClip;

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

    [Header("HIDE")]
    [Tooltip("Multiplier applied to MaxSpeed while hiding")]
    [Range(0.1f, 1f)]
    public float HideSpeedMultiplier = 0.4f;

    [Header("DARKNESS")]
    [Tooltip("Multiplier applied to MaxSpeed while inside an active DarknessZone")]
    [Range(0.1f, 1f)]
    public float DarknessSpeedMultiplier = 0.4f;

    [Header("SPIKES")]
    [Tooltip("Multiplier applied to MaxSpeed while standing on spikes")]
    [Range(0.1f, 1f)]
    public float SpikesSpeedMultiplier = 0.3f;

    [Header("PRESS TO WALL")]
    [Tooltip("Height multiplier for CapsuleCollider while pressed to wall")]
    [Range(0.2f, 1f)]
    public float PressToWallHeightMultiplier = 0.8f;

    [Tooltip("Width multiplier for CapsuleCollider while pressed to wall")]
    [Range(0.2f, 1f)]
    public float PressToWallWidthMultiplier = 0.5f;

    [Header("STUMBLE")]
    [Tooltip("Минимальная высота падения без кувырка, после которой игрок спотыкается")]
    [Min(0f)]
    public float StumbleMinFallHeight = 3.5f;

    [Tooltip("Сколько секунд длится анимация падения (скорость тормозит до нуля за это время)")]
    [Min(0f)]
    public float StumbleDuration = 1.2f;

    [Tooltip("Импульс по горизонтали вперёд в момент начала спотыкания")]
    [Min(0f)]
    public float StumbleImpulse = 3f;

    [Tooltip("Сколько секунд игрок хромает перед восстановлением")]
    [Min(0f)]
    public float LimpDuration = 3f;

    [Tooltip("Множитель скорости во время хромания (после падения, до захвата)")]
    [Range(0f, 1f)]
    public float LimpSpeedMultiplier = 0.35f;

    [Header("GRAB")]
    [Tooltip("HP drained per second while the player is grabbed")]
    [Min(0f)]
    public float GrabDamagePerSecond = 20f;

    [Tooltip("Rate at which grab progress drains per second when the player is not pressing buttons (in 0..1 units)")]
    [Min(0f)]
    public float GrabProgressDrainPerSecond = 0.15f;

    [Tooltip("Stamina regen multiplier during adrenaline burst triggered by being grabbed")]
    [Min(1f)]
    public float AdrenalineRegenMultiplier = 3f;

    [Tooltip("How long (seconds) the adrenaline burst lasts after being grabbed")]
    [Min(0f)]
    public float AdrenalineDuration = 8f;

    [Tooltip("Cooldown (seconds) after adrenaline expires before it can trigger again")]
    [Min(0f)]
    public float AdrenalineCooldown = 60f;

    [Header("LANDING")]
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

    [Header("FALL DAMAGE")]
    [Tooltip("Minimum fall height before taking damage")]
    [Min(0f)]
    public float FallDamageMinHeight = 10f;

    [Tooltip("Damage taken per unit of fall height")]
    [Min(0f)]
    public float FallDamagePerUnit = 5f;

    [Tooltip("Fall height that causes instant death")]
    [Min(0f)]
    public float FallDamageLethalHeight = 40f;

    [Tooltip("Damage multiplier when successfully rolling on landing (0-1, where 1 is no reduction)")]
    [Range(0f, 1f)]
    public float FallDamageRollMultiplier = 0.35f;

    [Header("CLIMB")]
    [Tooltip("Multiplier applied to (vertical) MaxSpeed while climbing")]
    [Range(0f, 2f)]
    public float ClimbVerticalSpeedMultiplier = 0.5f;

    [Tooltip("Multiplier applied to (horizontal) MaxSpeed while climbing")]
    [Range(0f, 2f)]
    public float ClimbHorizontalSpeedMultiplier = 0.5f;

    [Header("LIAN")]
    [Tooltip("Target downward speed while climbing on lianas")]
    [Min(0f)]
    public float VinesSlipSpeed = 2f;

    [Tooltip("How quickly the player slides down to the target liana slip speed")]
    [Min(0f)]
    public float VinesSlipAcceleration = 12f;

    [Header("LEDGE GRAB")]
    [Tooltip("Y offset from capsule bottom for the wall-check ray (should be about 0.7-0.9 of player height)")]
    [Min(0f)]
    public float LedgeWallCheckHeight = 1.0f;

    [Tooltip("Additional Y above the wall-check origin; that band must be free to confirm it's a ledge top")]
    [Min(0f)]
    public float LedgeTopCheckOffset = 0.25f;

    [Tooltip("Horizontal cast distance for ledge detection (added to capsule half-width)")]
    [Min(0f)]
    public float LedgeCheckDistance = 0.15f;

    [Tooltip("Extra gap between capsule edge and wall when hanging (0 = flush to wall)")]
    [Min(0f)]
    public float LedgeHangOffsetX = 0.05f;

    [Tooltip("Extra gap between capsule edge and wall corner when standing on ledge")]
    [Min(0f)]
    public float LedgeStandOffsetX = 0.05f;

    [Tooltip("Y offset from ledge corner for the stand position (player on top of ledge)")]
    [Min(0f)]
    public float LedgeStandOffsetY = 0.05f;

    [Tooltip("Delay before the player is teleported from the hang position to the top of the ledge")]
    [Min(0f)]
    public float LedgeClimbTeleportDelay = 0.4f;

    [Tooltip("How long the player can hang on a ledge while holding the grab button (0 = infinite)")]
    [Min(0f)]
    public float WallGrabMaxDuration = 7f;

    [Tooltip("How long the player can hang on a ledge without holding the grab button")]
    [Min(0f)]
    public float WallGrabPassiveDuration = 2f;

    [Tooltip("Maximum downward speed (positive value) at which ledge grab is allowed. Faster falling cancels the grab.")]
    [Min(0f)]
    public float LedgeGrabMaxFallSpeed = 8f;

    [Tooltip("Cooldown after releasing a ledge before the player can grab it again. Prevents instant re-grab when holding direction.")]
    [Min(0f)]
    public float LedgeGrabCooldown = 0.3f;

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

using UnityEngine;
using System;
using UnityEngine.Rendering;
using HSM;

[Serializable]
public class PlayerContext 
{
    // ===== INPUT & STATS =====
    public FrameInput input;
    public PlayerScriptableStats stats;
    public NoiseScriptableStats noiseStats;
    
    // ===== COMPONENTS =====
    public Animator anim;
    public Renderer renderer;
    public SortingOrderSetter sortingOrderSetter;
    public Collider2D coll;
    public Transform transform;
    //public AudioSource audio;
    public Rigidbody2D rb; // ???
    public PlayerInventory inventory; // ???
    public PlayerLightSensor lightSensor; // ???
    public CameraFollow cameraFollow;
    public HotbarController hotbar;
    
    // ===== MOVEMENT =====
    public bool grounded;
    public Vector2 velocity;
    public float currentSpeedMultiplier = 1f;
    public float currentNoiseRadius;
    public float currentFootstepInterval;
    public float airborneStartY;
    public float lastFallHeight;
    public float landingRollDirection;
    
    public bool isCrouching;
    public bool forcedCrouchAbove;
    public bool gateBlocksStanding;
    public Vector2 standingColliderSize;
    public Vector2 standingColliderOffset;

    public bool ceilingAbove;
    public bool isInteracting; // for Complex interactables (may be in future)
    
    public bool bufferedJumpUsable;
    public bool endedJumpEarly;
    public bool jumpToConsume;
    public bool coyoteUsable;
    
    // ===== HIDING =====
    public bool isHiding;
    public bool isPressedToWall;
    public bool isInNiche;
    
    // ===== STAMINA =====
    public float stamina;
    public float currentStaminaDrainMultiplier;
    public float currentStaminaBreathDrainMultiplier;
    public bool isHoldingBreath;
    
    // ===== LADDER =====
    public bool onLadder;
    public bool isClimbing;
    public bool onVines;

    public bool OnClimbable => onLadder || onVines;
    
    // ===== LEDGE GRAB =====
    public bool canGrabLedge;
    public bool isLedgeGrabbing;
    public Vector2 ledgeCornerPosition;
    public bool ledgeFacingRight;
    public float ledgeGrabCooldownEndTime = float.MinValue;
    public bool postClimbForcesCrouch;

    // ===== HEALTH =====
    public PlayerHealth health;
    public float currentHealth; // брать из PlayerHealth
    public bool isAlive;

    // ===== DARKNESS =====
    public bool isInDarkness;
    public float darknessDamagePerSecond;

    // ===== SPIKES =====
    public bool isOnSpikes;
    public float spikesDamage;

    // ===== GRABBED =====
    public bool isGrabbed;
    public bool isInDialog;

    // ===== ADRENALINE =====
    public float adrenalineEndTime = float.MinValue;
    public float adrenalineCooldownEndTime = float.MinValue;

    // ===== STUMBLE =====
    public bool stumblePending;
    public bool isStumbling;
    public bool isStumbleFalling;
    public bool grabEscapeDisabled;
    public float limpTimeRemaining;

    // ===== COSMETIC =====
    public Vector3 cosmeticOffset;

    // ===== SAFE ZONE =====
    public bool isInDetectionSafeZone;

    // ===== CUTSCENE =====
    public bool isInCutscene;

    // ===== SCENE ENTRY =====
    public bool isSceneEntry;
    public float sceneEntryMoveX;
    public int grabEscapeCount;
    public float grabProgress; // 0..1, доля выполненных нажатий для вырывания
    public bool diedWhileGrabbed;
    
    // ===== TIMERS =====
    public float timeInteractWasPressed;
    public float timeJumpWasPressed = float.MinValue;
    public float timeLastInteraction;
    public float frameLeftGrounded = float.MinValue;
    public float movementGraceTimer;
    public float timeRunStarted;
    public float timeCrouchWasPressed = float.MinValue;
    public float landingRollEndTime = float.MinValue;
    public float jumpBlockedUntil = float.MinValue;
     
    // ===== DERIVED PROPERTIES =====
    public bool HasBufferedJump => bufferedJumpUsable && stats != null && Time.time < timeJumpWasPressed + stats.JumpBuffer;
    public bool CanUseCoyote => coyoteUsable && !grounded && stats != null && Time.time < frameLeftGrounded + stats.CoyoteTime;
    public bool CanInteract => Time.time > timeLastInteraction + stats.InteractionCooldown;
    public bool CanRun => stamina > stats.MinStaminaToRun;
    public bool CanJump => stamina >= stats.JumpStaminaCost;
    public bool CanHoldBreath => currentStaminaBreathDrainMultiplier > 0f && currentStaminaDrainMultiplier == 0f;
    public bool HasMovementIntent => movementGraceTimer > 0f;
    public bool IsAdrenalineActive => Time.time < adrenalineEndTime;
    public bool IsAdrenalineOnCooldown => Time.time < adrenalineCooldownEndTime;
    public bool HasLandingRollBuffered => stats != null && Time.time < timeCrouchWasPressed + stats.LandingRollBuffer;
    public bool IsLandingRollActive => Time.time < landingRollEndTime;
    public bool WantsCrouch => input.CrouchHeld || IsLandingRollActive || forcedCrouchAbove || gateBlocksStanding;
    public ItemData SelectedHotbarItem => hotbar?.SelectedHotbarItem;
}

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
    
    // ===== COMPONENTS =====
    public Animator anim;
    public Renderer renderer;
    public Collider2D coll;
    public Transform transform;
    //public AudioSource audio;
    public Rigidbody2D rb; // ???
    public PlayerInventory inventory; // ???
    public PlayerLightSensor lightSensor; // ???
    public int selectedHotbarSlot = -1;
    public InventoryEntry selectedHotbarEntry;
    
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

    public bool ceilingAbove;
    public bool isInteracting; // for Complex interactables (may be in future)
    
    public bool bufferedJumpUsable;
    public bool endedJumpEarly;
    public bool jumpToConsume;
    public bool coyoteUsable;
    
    // ===== HIDING =====
    public bool isHiding;
    
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
    
    // ===== HEALTH =====
    public PlayerHealth health;
    public bool isAlive;
    
    // ===== TIMERS =====
    public float timeInteractWasPressed;
    public float timeJumpWasPressed;
    public float timeLastInteraction;
    public float frameLeftGrounded = float.MinValue;
    public float movementGraceTimer;
    public float timeRunStarted;
    public float timeCrouchWasPressed = float.MinValue;
    public float landingRollEndTime = float.MinValue;
     
    // ===== DERIVED PROPERTIES =====
    public bool HasBufferedJump => bufferedJumpUsable && stats != null && Time.time < timeJumpWasPressed + stats.JumpBuffer;
    public bool CanUseCoyote => coyoteUsable && !grounded && stats != null && Time.time < frameLeftGrounded + stats.CoyoteTime;
    public bool CanInteract => Time.time > timeLastInteraction + stats.InteractionCooldown && grounded; // Запрет на взаимодействие в воздухе
    public bool CanRun => stamina > stats.MinStaminaToRun;
    public bool CanHoldBreath => currentStaminaBreathDrainMultiplier > 0f && currentStaminaDrainMultiplier == 0f;
    public bool HasMovementIntent => movementGraceTimer > 0f;
    public bool HasLandingRollBuffered => stats != null && Time.time < timeCrouchWasPressed + stats.LandingRollBuffer;
    public bool IsLandingRollActive => Time.time < landingRollEndTime;
    public bool WantsCrouch => input.CrouchHeld || IsLandingRollActive;
    public ItemData SelectedHotbarItem => selectedHotbarEntry != null ? selectedHotbarEntry.item : null;
}

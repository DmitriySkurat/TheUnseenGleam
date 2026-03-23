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
    //public Rigidbody2D rb;
    public Renderer renderer;
    public Collider2D coll;
    public Transform transform;
    //public AudioSource audio;
    public PlayerInventory inventory; // ???
    public PlayerLightSensor lightSensor; // ???
    public int selectedHotbarSlot = -1;
    public InventoryEntry selectedHotbarEntry;
    
    // ===== MOVEMENT =====
    public Vector2 velocity;
    public float currentSpeedMultiplier = 1f;
    public float stamina = 100f;
    public float currentNoiseRadius;
    public float currentFootstepInterval;
    public float airbornePeakY;
    public float lastFallHeight;
    
    public bool isCrouching;
    public bool ceilingAbove;
    public bool isInteracting; // for Complex interactables (may be in future)
    
    public bool bufferedJumpUsable;
    public bool endedJumpEarly;
    public bool jumpToConsume;
    public bool coyoteUsable;
    
    // ===== HIDING =====
    public bool isHiding;
    
    
    // ===== LADDER =====
    public bool onLadder;
    public bool isClimbing;

    
    // ===== TIMERS =====
    public float timeInteractWasPressed;
    public float timeJumpWasPressed;
    public float timeLastInteraction;
    public float frameLeftGrounded = float.MinValue;
    public float movementGraceTimer;
    public float timeRunStarted;
    public float timeCrouchWasPressed = float.MinValue;
    public float landingRollEndTime = float.MinValue;
    
    // ===== PHYSICS =====
    public bool grounded;
     
    // ===== DERIVED PROPERTIES =====
    public bool HasBufferedJump => bufferedJumpUsable && stats != null && Time.time < timeJumpWasPressed + stats.JumpBuffer;
    public bool CanUseCoyote => coyoteUsable && !grounded && stats != null && Time.time < frameLeftGrounded + stats.CoyoteTime;
    
    // Запрет на взаимодействие в воздухе
    public bool CanInteract => Time.time > timeLastInteraction + stats.InteractionCooldown && grounded;
    public bool CanRun => stamina > stats.MinStaminaToRun;
    public bool HasMovementIntent => movementGraceTimer > 0f;
    public bool HasLandingRollBuffered => stats != null && Time.time < timeCrouchWasPressed + stats.LandingRollBuffer;
    public bool IsLandingRollActive => Time.time < landingRollEndTime;
    public bool WantsCrouch => input.CrouchHeld || IsLandingRollActive;
    public ItemData SelectedHotbarItem => selectedHotbarEntry != null ? selectedHotbarEntry.item : null;
}

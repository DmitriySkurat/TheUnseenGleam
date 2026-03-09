using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player;

    [Header("Profile")]
    [SerializeField] private PlayerNoiseProfile profile;

    [Header("Footstep Timing")]
    [SerializeField, Min(0.05f)] private float walkStepInterval = 0.5f;
    [SerializeField, Min(0.05f)] private float runStepInterval = 0.35f;
    [SerializeField, Min(0.05f)] private float crouchStepInterval = 0.7f;
    [SerializeField, Min(0.01f)] private float moveThreshold = 0.1f;

    private PlayerContext _ctx;
    private float _footstepTimer;
    private NoiseSystem _noiseSystem;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _noiseSystem = Services.Get<NoiseSystem>();
    }

    private void FixedUpdate()
    {
        if (_ctx == null || profile == null) return;

        HandleJumpNoise();
        HandleFootsteps(Time.fixedDeltaTime);
    }

    private void HandleJumpNoise()
    {
        if (!_ctx.jumpJustExecuted) return;

        // Emit a single jump noise when the jump is actually executed.
        _noiseSystem.EmitNoise(transform.position, profile.jumpRadius, gameObject, NoiseType.Jump);
        _ctx.jumpJustExecuted = false;
    }

    private void HandleFootsteps(float deltaTime)
    {
        if (_ctx.isClimbing)
        {
            _footstepTimer = 0f;
            return;
        }

        if (!_ctx.grounded || Mathf.Abs(_ctx.input.Move.x) < moveThreshold)
        {
            _footstepTimer = 0f;
            return;
        }

        // Determine movement mode for radius and cadence.
        bool isCrouching = _ctx.isCrouching;
        bool isRunning = _ctx.input.RunHeld && _ctx.CanRun && !isCrouching;

        float radius = isCrouching ? profile.crouchRadius : (isRunning ? profile.runRadius : profile.walkRadius);
        float interval = isCrouching ? crouchStepInterval : (isRunning ? runStepInterval : walkStepInterval);

        _footstepTimer += deltaTime;
        if (_footstepTimer < interval) return;

        _footstepTimer = 0f;
        _noiseSystem.EmitNoise(transform.position, radius, gameObject, NoiseType.Footstep);
    }
}

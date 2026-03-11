using UnityEngine;


public class PlayerInputHandler : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;

    private InputManager _inputManager;
    private PlayerContext _ctx;
    
    private FrameInput _frameInput = new FrameInput();
    public FrameInput FrameInput => _frameInput;
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _inputManager = Services.Get<InputManager>();
        
        SubscribeInput();
    }

    public void Dispose()
    {
        UnsubscribeInput();
    }

    void FixedUpdate()
    {
        _ctx.input = _frameInput;
    
        _frameInput.JumpDown = false;
        _frameInput.InteractDown = false;
    }

    void SubscribeInput() {
        if (_inputManager == null) return;

        _inputManager.OnMove += HandleMoveInput;
        _inputManager.OnJumpStarted += HandleJumpStarted;
        _inputManager.OnJumpCanceled += HandleJumpCanceled;
        _inputManager.OnCrouchToggled += HandleCrouchInput;
        _inputManager.OnRunToggled += HandleRunInput;
        
        _inputManager.OnInteractStarted += HandleInteractStarted;
        _inputManager.OnInteractCanceled += HandleInteractCanceled;
        
        _inputManager.OnLook += HandleLookInput;
        _inputManager.OnLookAroundToggled += HandleLookAroundInput;
    }

    void UnsubscribeInput() {
        if (_inputManager == null) return;

        _inputManager.OnMove -= HandleMoveInput;
        _inputManager.OnJumpStarted -= HandleJumpStarted;
        _inputManager.OnJumpCanceled -= HandleJumpCanceled;
        _inputManager.OnCrouchToggled -= HandleCrouchInput;
        _inputManager.OnRunToggled -= HandleRunInput;
        
        _inputManager.OnInteractStarted -= HandleInteractStarted;
        _inputManager.OnInteractCanceled -= HandleInteractCanceled;
        
        _inputManager.OnLook -= HandleLookInput;
        _inputManager.OnLookAroundToggled -= HandleLookAroundInput;
    }
    

    void HandleMoveInput(Vector2 direction) {
        _frameInput.Move = direction;

        if (_ctx == null || _ctx.stats == null || !_ctx.stats.SnapInput) return;

        _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _ctx.stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
        _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _ctx.stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
    }

    void HandleJumpStarted() {
        _frameInput.JumpDown = true;
        _frameInput.JumpHeld = true;

        _ctx.jumpToConsume = true;
        _ctx.timeJumpWasPressed = Time.time;
    }

    void HandleJumpCanceled() {
        _frameInput.JumpDown = false;
        _frameInput.JumpHeld = false;
    }
    
    void HandleCrouchInput(bool isCrouching)
    {
        _frameInput.CrouchHeld = isCrouching;
    }
    
    void HandleRunInput(bool isRunning)
    {
        _frameInput.RunHeld = isRunning;
    }
    
    void HandleInteractStarted()
    {
        _frameInput.InteractDown = true;
        _frameInput.InteractHeld = true;
        
        _ctx.timeInteractWasPressed = Time.time;
    }
    
    void HandleInteractCanceled()
    {
        _frameInput.InteractDown = false;
        _frameInput.InteractHeld = false;
    }
    
    void HandleLookInput(Vector2 mousePosition)
    {
        _frameInput.LookPosition = mousePosition;
    }
    
    void HandleLookAroundInput(bool isLookingAround)
    {
        _frameInput.LookAroundHeld = isLookingAround;
    }
}

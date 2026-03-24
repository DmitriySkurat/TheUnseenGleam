using UnityEngine;


public class PlayerInputHandler : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;

    private InputManager _inputManager;
    private FrameInput _frameInput;
    private PlayerContext _ctx;
    
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _inputManager = Services.Get<InputManager>();
        
        _frameInput = new FrameInput();
        SyncContextInput();
        
        SubscribeInput();
    }

    public void Dispose()
    {
        UnsubscribeInput();
    }

    void LateUpdate()
    {
        if (_ctx == null)
            return;

        _frameInput.JumpDown = false;
        _frameInput.InteractDown = false;
        _frameInput.AttackDown = false;
        _frameInput.SlotPressed = 0;

        SyncContextInput();
    }

    void SyncContextInput()
    {
        if (_ctx == null)
            return;

        _ctx.input = _frameInput;
    }

    void SubscribeInput() 
    {
        if (_inputManager == null) return;

        _inputManager.OnMove += HandleMoveInput;
        _inputManager.OnJumpStarted += HandleJumpStarted;
        _inputManager.OnJumpCanceled += HandleJumpCanceled;
        _inputManager.OnCrouchToggled += HandleCrouchInput;
        _inputManager.OnRunToggled += HandleRunInput;
        
        _inputManager.OnInteractStarted += HandleInteractStarted;
        _inputManager.OnInteractCanceled += HandleInteractCanceled;

        _inputManager.OnSlot1 += HandleSlot1;
        _inputManager.OnSlot2 += HandleSlot2;
        _inputManager.OnSlot3 += HandleSlot3;
        _inputManager.OnSlot4 += HandleSlot4;
        _inputManager.OnSlot5 += HandleSlot5;
        
        _inputManager.OnMousePositionChanged += HandleMousePosition;
        _inputManager.OnLookAroundToggled += HandleLookAroundInput;
        _inputManager.OnLMBStarted += HandleLMBStarted;
        _inputManager.OnLMBPerformed += HandleLMBPerformed;
        _inputManager.OnLMBCanceled += HandleLMBCanceled;
    }

    void UnsubscribeInput() 
    {
        if (_inputManager == null) return;

        _inputManager.OnMove -= HandleMoveInput;
        _inputManager.OnJumpStarted -= HandleJumpStarted;
        _inputManager.OnJumpCanceled -= HandleJumpCanceled;
        _inputManager.OnCrouchToggled -= HandleCrouchInput;
        _inputManager.OnRunToggled -= HandleRunInput;
        
        _inputManager.OnInteractStarted -= HandleInteractStarted;
        _inputManager.OnInteractCanceled -= HandleInteractCanceled;

        _inputManager.OnSlot1 -= HandleSlot1;
        _inputManager.OnSlot2 -= HandleSlot2;
        _inputManager.OnSlot3 -= HandleSlot3;
        _inputManager.OnSlot4 -= HandleSlot4;
        _inputManager.OnSlot5 -= HandleSlot5;
        
        _inputManager.OnMousePositionChanged -= HandleMousePosition;
        _inputManager.OnLookAroundToggled -= HandleLookAroundInput;
        _inputManager.OnLMBStarted -= HandleLMBStarted;
        _inputManager.OnLMBPerformed -= HandleLMBPerformed;
        _inputManager.OnLMBCanceled -= HandleLMBCanceled;
    }
    

    void HandleMoveInput(Vector2 direction) 
    {
        _frameInput.Move = direction;

        if (_ctx != null && _ctx.stats != null && _ctx.stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _ctx.stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _ctx.stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        SyncContextInput();
    }

    void HandleJumpStarted() 
    {
        _frameInput.JumpDown = true;
        _frameInput.JumpHeld = true;

        _ctx.jumpToConsume = true;
        _ctx.timeJumpWasPressed = Time.time;
        SyncContextInput();
    }

    void HandleJumpCanceled() 
    {
        _frameInput.JumpDown = false;
        _frameInput.JumpHeld = false;
        SyncContextInput();
    }
    
    void HandleCrouchInput(bool isCrouching)
    {
        if (isCrouching && !_frameInput.CrouchHeld)
        {
            _ctx.timeCrouchWasPressed = Time.time;
        }

        _frameInput.CrouchHeld = isCrouching;
        SyncContextInput();
    }
    
    void HandleRunInput(bool isRunning)
    {
        _frameInput.RunHeld = isRunning;
        SyncContextInput();
    }
    
    void HandleInteractStarted()
    {
        _frameInput.InteractDown = true;
        _frameInput.InteractHeld = true;
        
        _ctx.timeInteractWasPressed = Time.time;
        SyncContextInput();
    }
    
    void HandleInteractCanceled()
    {
        _frameInput.InteractDown = false;
        _frameInput.InteractHeld = false;
        SyncContextInput();
    }

    void HandleSlot1() { _frameInput.SlotPressed = 1; SyncContextInput(); }
    void HandleSlot2() { _frameInput.SlotPressed = 2; SyncContextInput(); }
    void HandleSlot3() { _frameInput.SlotPressed = 3; SyncContextInput(); }
    void HandleSlot4() { _frameInput.SlotPressed = 4; SyncContextInput(); }
    void HandleSlot5() { _frameInput.SlotPressed = 5; SyncContextInput(); }
    
    
    // Mouse
    void HandleMousePosition(Vector2 mousePosition)
    {
        _frameInput.MousePosition = mousePosition;
        SyncContextInput();
    }
    
    void HandleLookAroundInput(bool isLookingAround)
    {
        _frameInput.LookAroundHeld = isLookingAround;
        SyncContextInput();
    }

    void HandleLMBStarted()
    {
        _frameInput.AttackDown = true;
        SyncContextInput();
    }

    void HandleLMBPerformed()
    {
        _frameInput.AttackHeld = true;
        SyncContextInput();
    }

    void HandleLMBCanceled()
    {
        _frameInput.AttackHeld = false;
        SyncContextInput();
    }
}

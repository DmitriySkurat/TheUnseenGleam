using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;


public class InputManager : MonoBehaviour, IService
{
    // Keyboard
    public event Action<Vector2> OnMove;
    public event Action OnJumpStarted;
    public event Action OnJumpCanceled;
    public event Action<bool> OnRunToggled;
    public event Action<bool> OnCrouchToggled;
    public event Action<bool> OnHoldBreathToggled;
    
    public event Action OnInteractStarted;
    public event Action OnInteractCanceled;
    
    public event Action OnSlot1;
    public event Action OnSlot2;
    public event Action OnSlot3;
    public event Action OnSlot4;
    public event Action OnSlot5;
    
    public event Action OnEscape;
    public event Action OnCloseWindow;
    
    
    // Mouse
    public event Action<Vector2> OnMousePositionChanged;
    public event Action<bool> OnLookAroundToggled;
    public event Action OnLMBStarted;
    public event Action OnLMBPerformed;
    public event Action OnLMBCanceled;
    
    private PlayerInput _playerInput;
    
    
    private bool _initialized;
    
    public async Task InitializeAsync()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        if (_playerInput == null) {
            Debug.LogError("PlayerInput component not found on InputManager!", this);
            return;
        }
        
        RegisterInputs();
        EnableGameplay();
        
        _initialized = true;
        
        await Task.CompletedTask;
    }
    
    private void OnDestroy()
    {
        if (_initialized)
            UnregisterInputs();
    }
    
    private void RegisterInputs()
    {
        var actions = _playerInput.actions;

        actions["Move"].performed += HandleMove;
        actions["Move"].canceled += HandleMove;

        actions["Jump"].started += HandleJumpStarted;
        actions["Jump"].canceled += HandleJumpCanceled;

        actions["Run"].performed += HandleRun;
        actions["Run"].canceled += HandleRun;

        actions["Crouch"].performed += HandleCrouch;
        actions["Crouch"].canceled += HandleCrouch;
        
        actions["HoldBreath"].performed += HandleHoldBreath;
        actions["HoldBreath"].canceled += HandleHoldBreath;
        
        actions["Interact"].performed += HandleInteractStarted;
        actions["Interact"].canceled += HandleInteractCanceled;

        actions["Slot1"].performed += HandleSlot1;
        actions["Slot2"].performed += HandleSlot2;
        actions["Slot3"].performed += HandleSlot3;
        actions["Slot4"].performed += HandleSlot4;
        actions["Slot5"].performed += HandleSlot5;
        
        actions["Escape"].performed += HandleOnEscape;
        actions["CloseWindow"].performed += HandleOnCloseWindow;
        
        
        actions["MousePosition"].performed += HandleMousePosition;
        actions["MousePosition"].canceled += HandleMousePosition;
        
        actions["LookAround"].performed += HandleLookAround;
        actions["LookAround"].canceled += HandleLookAround;
        
        actions["Attack"].started += HandleLMBStarted;
        actions["Attack"].performed += HandleLMBPerformed;
        actions["Attack"].canceled += HandleLMBCanceled;
    }
    
    private void UnregisterInputs()
    {
        var actions = _playerInput.actions;

        actions["Move"].performed -= HandleMove;
        actions["Move"].canceled -= HandleMove;

        actions["Jump"].started -= HandleJumpStarted;
        actions["Jump"].canceled -= HandleJumpCanceled;

        actions["Run"].performed -= HandleRun;
        actions["Run"].canceled -= HandleRun;

        actions["Crouch"].performed -= HandleCrouch;
        actions["Crouch"].canceled -= HandleCrouch;
        
        actions["Interact"].performed -= HandleInteractStarted;
        actions["Interact"].canceled -= HandleInteractCanceled;
        
        actions["HoldBreath"].performed -= HandleHoldBreath;
        actions["HoldBreath"].canceled -= HandleHoldBreath;

        actions["Slot1"].performed -= HandleSlot1;
        actions["Slot2"].performed -= HandleSlot2;
        actions["Slot3"].performed -= HandleSlot3;
        actions["Slot4"].performed -= HandleSlot4;
        actions["Slot5"].performed -= HandleSlot5;
        
        actions["Escape"].performed -= HandleOnEscape;
        actions["CloseWindow"].performed -= HandleOnCloseWindow;
        
        
        actions["MousePosition"].performed -= HandleMousePosition;
        actions["MousePosition"].canceled -= HandleMousePosition;
        
        actions["LookAround"].performed -= HandleLookAround;
        actions["LookAround"].canceled -= HandleLookAround;
        
        actions["Attack"].started -= HandleLMBStarted;
        actions["Attack"].performed -= HandleLMBPerformed;
        actions["Attack"].canceled -= HandleLMBCanceled;
    }


    public void EnableGameplay()
    {
        _playerInput.SwitchCurrentActionMap("Gameplay");
        SetCursorVisibility(false);
    }

    public void SetLookAroundEnabled(bool enabled)
    {
        var action = _playerInput.actions["LookAround"];
        if (enabled) action.Enable();
        else action.Disable();
    }

    public void EnableUI()
    {
        _playerInput.SwitchCurrentActionMap("UI");
        SetCursorVisibility(true);
    }

    public void SetCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
    }
    
    // void Update()
    //     {
    //         Services.Update();
    //     }

    #region Input Handlers
    
    // Keyboard
    private void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void HandleJumpStarted(InputAction.CallbackContext ctx) => OnJumpStarted?.Invoke();
    private void HandleJumpCanceled(InputAction.CallbackContext ctx) => OnJumpCanceled?.Invoke();
    private void HandleRun(InputAction.CallbackContext ctx) => OnRunToggled?.Invoke(ctx.performed);
    private void HandleCrouch(InputAction.CallbackContext ctx) => OnCrouchToggled?.Invoke(ctx.performed);
    
    private void HandleHoldBreath(InputAction.CallbackContext ctx) => OnHoldBreathToggled?.Invoke(ctx.performed);
    
    private void HandleInteractStarted(InputAction.CallbackContext ctx) => OnInteractStarted?.Invoke();
    private void HandleInteractCanceled(InputAction.CallbackContext ctx) => OnInteractCanceled?.Invoke();

    private void HandleSlot1(InputAction.CallbackContext ctx) => OnSlot1?.Invoke();
    private void HandleSlot2(InputAction.CallbackContext ctx) => OnSlot2?.Invoke();
    private void HandleSlot3(InputAction.CallbackContext ctx) => OnSlot3?.Invoke();
    private void HandleSlot4(InputAction.CallbackContext ctx) => OnSlot4?.Invoke();
    private void HandleSlot5(InputAction.CallbackContext ctx) => OnSlot5?.Invoke();
    
    private void HandleOnEscape(InputAction.CallbackContext ctx) => OnEscape?.Invoke();
    private void HandleOnCloseWindow(InputAction.CallbackContext ctx) => OnCloseWindow?.Invoke();
    
    // Mouse
    private void HandleMousePosition(InputAction.CallbackContext ctx) => OnMousePositionChanged?.Invoke(ctx.ReadValue<Vector2>());
    private void HandleLookAround(InputAction.CallbackContext ctx) => OnLookAroundToggled?.Invoke(ctx.performed);
    
    private void HandleLMBStarted(InputAction.CallbackContext ctx) => OnLMBStarted?.Invoke();
    private void HandleLMBPerformed(InputAction.CallbackContext ctx) => OnLMBPerformed?.Invoke();
    private void HandleLMBCanceled(InputAction.CallbackContext ctx) => OnLMBCanceled?.Invoke();
    
    #endregion
}

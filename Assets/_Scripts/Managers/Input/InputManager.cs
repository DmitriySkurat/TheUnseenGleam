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
    
    public event Action OnInteractStarted;
    public event Action OnInteractCanceled;
    
    
    // Mouse
    public event Action<Vector2> OnMousePositionChanged;
    public event Action<bool> OnLookAroundToggled;
    public event Action<bool> OnAttackToggled;
    
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
        
        actions["Interact"].performed += HandleInteractStarted;
        actions["Interact"].canceled += HandleInteractCanceled;
        
        actions["MousePosition"].performed += HandleMousePosition;
        actions["MousePosition"].canceled += HandleMousePosition;
        
        actions["LookAround"].performed += HandleLookAround;
        actions["LookAround"].canceled += HandleLookAround;
        
        actions["Attack"].performed += HandleAttack;
        actions["Attack"].canceled += HandleAttack;
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
        
        actions["MousePosition"].performed -= HandleMousePosition;
        actions["MousePosition"].canceled -= HandleMousePosition;
        
        actions["LookAround"].performed -= HandleLookAround;
        actions["LookAround"].canceled -= HandleLookAround;
        
        actions["Attack"].performed -= HandleAttack;
        actions["Attack"].canceled -= HandleAttack;
    }


    public void EnableGameplay()
    {
        _playerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void EnableUI()
    {
        _playerInput.SwitchCurrentActionMap("UI");
    }

    #region Input Handlers
    
    // Keyboard
    private void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void HandleJumpStarted(InputAction.CallbackContext ctx) => OnJumpStarted?.Invoke();
    private void HandleJumpCanceled(InputAction.CallbackContext ctx) => OnJumpCanceled?.Invoke();
    private void HandleRun(InputAction.CallbackContext ctx) => OnRunToggled?.Invoke(ctx.performed);
    private void HandleCrouch(InputAction.CallbackContext ctx) => OnCrouchToggled?.Invoke(ctx.performed);
    
    private void HandleInteractStarted(InputAction.CallbackContext ctx) => OnInteractStarted?.Invoke();
    private void HandleInteractCanceled(InputAction.CallbackContext ctx) => OnInteractCanceled?.Invoke();
    
    // Mouse
    private void HandleMousePosition(InputAction.CallbackContext ctx) => OnMousePositionChanged?.Invoke(ctx.ReadValue<Vector2>());
    private void HandleLookAround(InputAction.CallbackContext ctx) => OnLookAroundToggled?.Invoke(ctx.performed);
    private void HandleAttack(InputAction.CallbackContext ctx) => OnAttackToggled?.Invoke(ctx.performed);
    
    #endregion
}

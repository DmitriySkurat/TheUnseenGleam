using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action OnJumpStarted;
    public event Action OnJumpCanceled;
    public event Action<bool> OnRunToggled;
    public event Action<bool> OnCrouchToggled;
    
    public event Action<bool> OnInteractToggled;
    
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        RegisterInputs();
    }

    private void OnDisable()
    {
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
        
        actions["Interact"].performed += HandleInteract;
        actions["Interact"].canceled += HandleInteract;
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
        
        actions["Interact"].performed -= HandleInteract;
        actions["Interact"].canceled -= HandleInteract;
    }

    private void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void HandleJumpStarted(InputAction.CallbackContext ctx) => OnJumpStarted?.Invoke();
    private void HandleJumpCanceled(InputAction.CallbackContext ctx) => OnJumpCanceled?.Invoke();
    private void HandleRun(InputAction.CallbackContext ctx) => OnRunToggled?.Invoke(ctx.performed);
    private void HandleCrouch(InputAction.CallbackContext ctx) => OnCrouchToggled?.Invoke(ctx.performed);
    private void HandleInteract(InputAction.CallbackContext ctx) => OnInteractToggled?.Invoke(ctx.performed);
}

using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;
    
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool CrouchIsHeld;
    
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _crouchAction;
    
    
    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _crouchAction = PlayerInput.actions["Crouch"];
    }
    
    void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        
        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
        CrouchIsHeld = _crouchAction.IsPressed();
    }
}

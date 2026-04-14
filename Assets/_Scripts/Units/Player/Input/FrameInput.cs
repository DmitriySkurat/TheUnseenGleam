using UnityEngine;

public struct FrameInput
{
    public Vector2 Move;
    public bool JumpDown;
    public bool JumpHeld;
    public bool CrouchHeld;
    public bool RunHeld;
    public bool HoldBreathHeld;
    
    public bool InteractDown;
    public bool InteractHeld;

    public int SlotPressed;
    
    
    public Vector2 MousePosition;
    public bool LookAroundHeld;
    public bool AttackHeld;
    public bool AttackDown;
}

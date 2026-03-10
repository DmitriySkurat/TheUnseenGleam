using UnityEngine;

public struct FrameInput
{
    public Vector2 Move;
    public bool JumpDown;
    public bool JumpHeld;
    public bool CrouchHeld;
    public bool RunHeld;
    
    public bool InteractDown;
    public bool InteractHeld;
    
    public Vector2 LookPosition;
    public bool LookAroundHeld;
}
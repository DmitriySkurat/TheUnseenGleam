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

    public bool Slot1Down;
    public bool Slot2Down;
    public bool Slot3Down;
    public bool Slot4Down;
    public bool Slot5Down;
    
    public Vector2 MousePosition;
    public bool LookAroundHeld;
    public bool AttackHeld;
}

using UnityEngine;

public static class PlayerAnimations
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Walk = Animator.StringToHash("Walk");
    public static readonly int Run = Animator.StringToHash("Run");
    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int Dropdown = Animator.StringToHash("Dropdown");
    public static readonly int CrouchIdle = Animator.StringToHash("CrouchIdle");
    public static readonly int CrouchWalk = Animator.StringToHash("CrouchWalk");
    public static readonly int Climb = Animator.StringToHash("Climb");
    public static readonly int ClimbIdle = Animator.StringToHash("ClimbIdle");
    public static readonly int JumpWallgrab = Animator.StringToHash("JumpWallgrab");
    public static readonly int WallgrabClime = Animator.StringToHash("WallgrabClime");
    public static readonly int WallgrabJumpOff = Animator.StringToHash("WallgrabJumpOff");
    public static readonly int PressToWall = Animator.StringToHash("PressToWall");
    public static readonly int Death = Animator.StringToHash("Death");
    public static readonly int Hide = Animator.StringToHash("Hide");
    public static readonly int Grabbed = Animator.StringToHash("Grabbed");
    public static readonly int Interact = Animator.StringToHash("Interact");
    public static readonly int Stop    = Animator.StringToHash("Stop");
    public static readonly int Stumble          = Animator.StringToHash("Stumble");
    public static readonly int Limp             = Animator.StringToHash("Limp");
    public static readonly int WalkAfterStumble = Animator.StringToHash("WalkAfterStumble");
}

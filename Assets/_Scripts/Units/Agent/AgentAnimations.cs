using UnityEngine;

public static class AgentAnimations
{
    public static readonly int Idle        = Animator.StringToHash("Idle");
    public static readonly int Walk        = Animator.StringToHash("Walk");
    public static readonly int Run         = Animator.StringToHash("Run");
    public static readonly int Jump        = Animator.StringToHash("Jump");
    public static readonly int Dropdown    = Animator.StringToHash("Dropdown");
    public static readonly int Suspicious  = Animator.StringToHash("Suspicious");
    public static readonly int Alert       = Animator.StringToHash("Alert");
    public static readonly int GrabPlayer  = Animator.StringToHash("GrabPlayer");
}

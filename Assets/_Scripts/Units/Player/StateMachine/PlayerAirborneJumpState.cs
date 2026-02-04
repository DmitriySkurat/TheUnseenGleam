using UnityEngine;

public sealed class PlayerAirborneJumpState : PlayerAirborneStateBase
{
    public PlayerAirborneJumpState(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnEnter()
    {
        Debug.Log("Enter Airborne Jump");
    }

    protected override void OnExit()
    {
        Debug.Log("Exit Airborne Jump");
    }
}

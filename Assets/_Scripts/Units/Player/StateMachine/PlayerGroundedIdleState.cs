using UnityEngine;

public sealed class PlayerGroundedIdleState : PlayerGroundedStateBase
{
    public PlayerGroundedIdleState(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnEnter()
    {
        Debug.Log("Enter Grounded Idle");
    }

    protected override void OnExit()
    {
        Debug.Log("Exit Grounded Idle");
    }
}

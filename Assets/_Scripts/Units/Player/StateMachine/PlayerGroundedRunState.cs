using UnityEngine;

public sealed class PlayerGroundedRunState : PlayerGroundedStateBase
{
    public PlayerGroundedRunState(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnEnter()
    {
        Debug.Log("Enter Grounded Run");
    }

    protected override void OnExit()
    {
        Debug.Log("Exit Grounded Run");
    }

    protected override float GetMaxSpeed()
    {
        return Stats.MaxSpeed * Stats.RunSpeedMultiplier;
    }
}

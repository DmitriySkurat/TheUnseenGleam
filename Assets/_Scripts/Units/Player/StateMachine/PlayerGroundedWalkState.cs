using UnityEngine;

public sealed class PlayerGroundedWalkState : PlayerGroundedStateBase
{
    public PlayerGroundedWalkState(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnEnter()
    {
        Debug.Log("Enter Grounded Walk");
    }

    protected override void OnExit()
    {
        Debug.Log("Exit Grounded Walk");
    }

    protected override float GetMaxSpeed()
    {
        return Stats.MaxSpeed * Stats.WalkSpeedMultiplier;
    }
}

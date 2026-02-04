using UnityEngine;

public sealed class PlayerGroundedCrouchState : PlayerGroundedStateBase
{
    public PlayerGroundedCrouchState(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnEnter()
    {
        Debug.Log("Enter Grounded Crouch");
        Controller.SetCrouchCollider(true);
    }

    protected override void OnExit()
    {
        Debug.Log("Exit Grounded Crouch");
        Controller.SetCrouchCollider(false);
    }

    protected override float GetMaxSpeed()
    {
        return Stats.MaxSpeed * Stats.CrouchSpeedMultiplier;
    }
}

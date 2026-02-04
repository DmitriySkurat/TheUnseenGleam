using UnityEngine;

public sealed class PlayerAirborneFallState : PlayerAirborneStateBase
{
    public PlayerAirborneFallState(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnEnter()
    {
        Debug.Log("Enter Airborne Fall");
    }

    protected override void OnExit()
    {
        Debug.Log("Exit Airborne Fall");
    }
}

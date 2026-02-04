public sealed class PlayerAirborneState : PlayerStateBase
{
    private readonly PlayerAirborneJumpState _jumpState;
    private readonly PlayerAirborneFallState _fallState;
    private PlayerGroundedState _groundedState;

    public PlayerAirborneState(PlayerController controller, Hsm hsm) : base(controller, hsm)
    {
        IsRootState = true;
        _jumpState = new PlayerAirborneJumpState(controller, hsm);
        _fallState = new PlayerAirborneFallState(controller, hsm);
    }

    public void SetGroundedState(PlayerGroundedState groundedState)
    {
        _groundedState = groundedState;
    }

    protected override void InitializeSubState()
    {
        if (Rigidbody.linearVelocity.y >= 0f)
        {
            SetSubState(_jumpState);
        }
        else
        {
            SetSubState(_fallState);
        }
    }

    protected override void OnUpdate()
    {
        if (Controller.IsGrounded)
        {
            SwitchState(_groundedState);
            return;
        }

        if (Rigidbody.linearVelocity.y >= 0f)
        {
            SwitchSubState(_jumpState);
        }
        else
        {
            SwitchSubState(_fallState);
        }
    }
}

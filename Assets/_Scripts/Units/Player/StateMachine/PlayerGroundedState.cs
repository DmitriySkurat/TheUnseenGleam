using UnityEngine;

public sealed class PlayerGroundedState : PlayerStateBase
{
    private const float MoveDeadzone = 0.01f;
    private readonly PlayerGroundedIdleState _idleState;
    private readonly PlayerGroundedWalkState _walkState;
    private readonly PlayerGroundedRunState _runState;
    private readonly PlayerGroundedCrouchState _crouchState;
    private PlayerAirborneState _airborneState;

    public PlayerGroundedState(PlayerController controller, Hsm hsm) : base(controller, hsm)
    {
        IsRootState = true;
        _idleState = new PlayerGroundedIdleState(controller, hsm);
        _walkState = new PlayerGroundedWalkState(controller, hsm);
        _runState = new PlayerGroundedRunState(controller, hsm);
        _crouchState = new PlayerGroundedCrouchState(controller, hsm);
    }

    public void SetAirborneState(PlayerAirborneState airborneState)
    {
        _airborneState = airborneState;
    }

    protected override void InitializeSubState()
    {
        SetSubState(SelectSubState());
    }

    protected override void OnUpdate()
    {
        if (!Controller.IsGrounded)
        {
            SwitchState(_airborneState);
            return;
        }

        SwitchSubState(SelectSubState());
    }

    private PlayerStateBase SelectSubState()
    {
        if (Controller.CurrentFrameInput.CrouchHeld)
        {
            return _crouchState;
        }

        if (Mathf.Abs(Controller.CurrentFrameInput.Move.x) > MoveDeadzone)
        {
            return Controller.CurrentFrameInput.RunHeld ? _runState : _walkState;
        }

        return _idleState;
    }
}

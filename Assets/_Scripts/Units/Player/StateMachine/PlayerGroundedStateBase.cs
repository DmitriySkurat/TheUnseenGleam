public abstract class PlayerGroundedStateBase : PlayerStateBase
{
    protected PlayerGroundedStateBase(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnFixedUpdate()
    {
        HandleJump();
        HandleDirection();
        HandleGravity();
        ApplyMovement();
    }
}

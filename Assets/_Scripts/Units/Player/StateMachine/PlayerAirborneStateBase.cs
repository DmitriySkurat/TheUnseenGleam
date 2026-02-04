public abstract class PlayerAirborneStateBase : PlayerStateBase
{
    protected PlayerAirborneStateBase(PlayerController controller, Hsm hsm) : base(controller, hsm) { }

    protected override void OnFixedUpdate()
    {
        HandleJump();
        HandleDirection();
        HandleGravity();
        ApplyMovement();
    }
}

namespace HSM {
    public abstract class GroundedStateBase : Player2DState {
        protected GroundedStateBase(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) { }

        protected override void OnUpdate(float deltaTime) {
            HandleJump(deltaTime);
            HandleDirection(deltaTime);
            HandleGravity(deltaTime);
            ApplyMovement();
        }
    }
}

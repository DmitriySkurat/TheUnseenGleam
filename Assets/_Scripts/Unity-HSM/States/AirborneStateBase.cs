namespace HSM {
    public abstract class AirborneStateBase : Player2DState {
        protected AirborneStateBase(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) { }

        protected override void OnUpdate(float deltaTime) {
            HandleJump(deltaTime);
            HandleDirection(deltaTime);
            HandleGravity(deltaTime);
            ApplyMovement();
        }
    }
}

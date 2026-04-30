using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        
        public readonly Idle Idle;
        public readonly Stopping Stopping;
        public readonly Crouch Crouch;
        public readonly Move Move;
        public readonly Hide Hide;
        public readonly PressToWall PressToWall;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Idle        = new Idle(m, this, ctx);
            Stopping    = new Stopping(m, this, ctx);
            Crouch      = new Crouch(m, this, ctx);
            Move        = new Move(m, this, ctx);
            Hide        = new Hide(m, this, ctx);
            PressToWall = new PressToWall(m, this, ctx);
            
            // Add(new ColorPhaseActivity(ctx.renderer){
            //     enterColor = Color.yellow,  // runs while Grounded is activating
            // });
        }
        
        protected override State GetInitialState() {
            if (ctx.HasMovementIntent) return Move;
            if (ctx.stats != null && Mathf.Abs(ctx.velocity.x) > ctx.stats.MaxSpeed * ctx.stats.WalkSpeedMultiplier) return Stopping;
            return Idle;
        }

        protected override State GetTransition()
        {
            if (ctx.OnClimbable && ctx.input.Move.y > 0.1f) return Machine.GetState<Climb>();
            if (ctx.OnClimbable && ctx.input.Move.y < -0.1f) return Machine.GetState<Climb>();
            if (!ctx.grounded && Time.time >= ctx.frameLeftGrounded + Time.fixedDeltaTime) return Machine != null ? Machine.GetState<Airborne>() : null;

            // W нажат, не на лестнице → прижаться к стене
            if (!ctx.isHiding && ctx.input.Move.y > ctx.stats.VerticalDeadZoneThreshold && !ctx.OnClimbable)
                return Machine != null ? Machine.GetState<PressToWall>() : null;

            return null;
        }
        
        
        protected override void OnUpdate(float deltaTime)
        {
            HandleHorizontal(deltaTime);
            base.OnUpdate(deltaTime);
        }

        void HandleHorizontal(float deltaTime)
        {
            if (ctx.stats == null) return;

            if (ctx.IsLandingRollActive)
            {
                float direction = ctx.landingRollDirection != 0f ? Mathf.Sign(ctx.landingRollDirection) : Mathf.Sign(ctx.velocity.x);
                float speed = Mathf.Max(0f, Mathf.Abs(ctx.velocity.x) - ctx.stats.LandingRollDeceleration * deltaTime);
                ctx.velocity.x = speed * direction;
                return;
            }

            float targetSpeed = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier;

            float accel = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;

            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetSpeed, accel * deltaTime);
        }
    }
}

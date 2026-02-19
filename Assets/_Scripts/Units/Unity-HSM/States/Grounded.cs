using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        public readonly Idle Idle;
        public readonly Move Move;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Idle = new Idle(m, this, ctx);
            Move = new Move(m, this, ctx);
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.yellow,  // runs while Grounded is activating
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Grounded", true, false));
        }
        
        protected override State GetInitialState() => Idle;

        protected override State GetTransition() {
            return ctx.grounded ? null : ((PlayerRoot)Parent).Airborne;
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats == null) return;
            HandleJump();
            HandleGravity(deltaTime);
        }

        void HandleJump() {
            if (!ctx.endedJumpEarly && !ctx.grounded && !ctx.jumpHeld && ctx.velocity.y > 0) ctx.endedJumpEarly = true;

            if (!ctx.jumpToConsume && !ctx.HasBufferedJump) return;

            if (ctx.grounded || ctx.CanUseCoyote) ExecuteJump();

            ctx.jumpToConsume = false;
        }

        void ExecuteJump() {
            ctx.endedJumpEarly = false;
            ctx.timeJumpWasPressed = 0;
            ctx.bufferedJumpUsable = false;
            ctx.coyoteUsable = false;
            ctx.velocity.y = ctx.stats.JumpPower;
        }

        void HandleGravity(float deltaTime) {
            if (ctx.grounded && ctx.velocity.y <= 0f) {
                ctx.velocity.y = ctx.stats.GroundingForce;
            } else {
                var inAirGravity = ctx.stats.FallAcceleration;
                if (ctx.endedJumpEarly && ctx.velocity.y > 0) inAirGravity *= ctx.stats.JumpEndEarlyGravityModifier;
                ctx.velocity.y = Mathf.MoveTowards(ctx.velocity.y, -ctx.stats.MaxFallSpeed, inAirGravity * deltaTime);
            }
        }
    }
}

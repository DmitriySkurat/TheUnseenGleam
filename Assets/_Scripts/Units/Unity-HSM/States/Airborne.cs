using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext ctx;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red, // runs while Airborne is activating
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Airborne", true, false));
        }
        
        protected override State GetTransition() => ctx.grounded ? ((PlayerRoot)Parent).Grounded : null;

        protected override void OnEnter() {
            // TODO: Update Animator through ctx.anim
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats == null) return;
            HandleJump();
            HandleHorizontal(deltaTime);
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

        void HandleHorizontal(float deltaTime) {
            if (Mathf.Abs(ctx.move.x) <= 0.01f) {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, ctx.stats.AirDeceleration * deltaTime);
            } else {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, ctx.move.x * ctx.stats.MaxSpeed, ctx.stats.Acceleration * deltaTime);
            }
        }

        void HandleGravity(float deltaTime) {
            var inAirGravity = ctx.stats.FallAcceleration;
            if (ctx.endedJumpEarly && ctx.velocity.y > 0) inAirGravity *= ctx.stats.JumpEndEarlyGravityModifier;
            ctx.velocity.y = Mathf.MoveTowards(ctx.velocity.y, -ctx.stats.MaxFallSpeed, inAirGravity * deltaTime);
        }
    }
}

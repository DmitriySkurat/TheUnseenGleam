using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        
        public readonly Idle Idle;
        public readonly Crouch Crouch;
        public readonly Move Move;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Idle = new Idle(m, this, ctx);
            Crouch = new Crouch(m, this, ctx);
            Move = new Move(m, this, ctx);
            
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.yellow,  // runs while Grounded is activating
            });
        }
        
        protected override State GetInitialState() => Idle;

        protected override State GetTransition() => ctx.grounded ? null : (Machine != null ? Machine.GetState<Airborne>() : null);
        
        
        protected override void OnUpdate(float deltaTime)
        {
            HandleHorizontal(deltaTime);
            base.OnUpdate(deltaTime);
        }

        void HandleHorizontal(float deltaTime)
        {
            if (ctx.stats == null) return;

            float targetSpeed = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier;

            float accel = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;

            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetSpeed, accel * deltaTime);
        }
    }
}

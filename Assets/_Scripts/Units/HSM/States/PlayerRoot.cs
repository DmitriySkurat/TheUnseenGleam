using UnityEngine;

namespace HSM {
    public class PlayerRoot : State {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;
        public readonly Interaction Interaction;
        public readonly Climb Climb;
        
        readonly PlayerContext ctx;

        public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m, null) {
            this.ctx = ctx;
            Grounded = new Grounded(m, this, ctx);
            Airborne = new Airborne(m, this, ctx);
            Interaction = new Interaction(m, this, ctx);
            Climb = new Climb(m, this, ctx);
        }
        
        protected override State GetInitialState() => Grounded;
        protected override State GetTransition()
        {
            if (ctx.onLadder) return null;
            if (!ctx.grounded) return Machine != null ? Machine.GetState<Airborne>() : null;
            
            return null;
        } 

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats != null) {
                HandleJump();
                StaminaRecovery(deltaTime);
            }
            base.OnUpdate(deltaTime);
        }

        void HandleJump() {
            if (!ctx.endedJumpEarly && !ctx.grounded && !ctx.input.JumpHeld && ctx.velocity.y > 0) ctx.endedJumpEarly = true;
            
            if (ctx.ceilingAbove && ctx.isCrouching)
            {
                ctx.jumpToConsume = false;
                return;
            }

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

        
        
        void StaminaRecovery(float deltaTime)
        {
            if (!(Machine.Root.Leaf() is Run))
            {
                ctx.stamina += ctx.stats.StaminaRegenPerSecond * deltaTime;
                ctx.stamina = Mathf.Min(ctx.stamina, ctx.stats.MaxStamina);
            }
        }
    }
}
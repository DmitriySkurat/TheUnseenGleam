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
                HandleGravity(deltaTime);
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

        void HandleGravity(float deltaTime) {
        
            if (ctx.isClimbing)
                return;
    
            if (ctx.grounded && ctx.velocity.y <= 0f) {
                ctx.velocity.y = ctx.stats.GroundingForce;
            } else {
                var inAirGravity = ctx.stats.FallAcceleration;
                if (ctx.endedJumpEarly && ctx.velocity.y > 0) inAirGravity *= ctx.stats.JumpEndEarlyGravityModifier;
                ctx.velocity.y = Mathf.MoveTowards(ctx.velocity.y, -ctx.stats.MaxFallSpeed, inAirGravity * deltaTime);
            }
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
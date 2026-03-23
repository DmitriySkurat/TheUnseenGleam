using UnityEngine;

namespace HSM {
    public class PlayerRoot : State {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;
        public readonly Interaction Interaction;
        public readonly Climb Climb;
        
        readonly PlayerContext ctx;
        private float _footstepTimer;
        
        private NoiseSystem _noiseSystem;
        

        public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m, null) {
            this.ctx = ctx;
            Grounded = new Grounded(m, this, ctx);
            Airborne = new Airborne(m, this, ctx);
            Interaction = new Interaction(m, this, ctx);
            Climb = new Climb(m, this, ctx);
            
            _noiseSystem = Services.Get<NoiseSystem>();
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
                HandleFootsteps(deltaTime);
            }
            base.OnUpdate(deltaTime);
        }

        void HandleJump() {
            if (ctx.isHiding)
            {
                ctx.jumpToConsume = false;
                return;
            }

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
            ctx.landingRollEndTime = float.MinValue;
            ctx.landingRollDirection = 0f;
            ctx.velocity.y = ctx.stats.JumpPower;

            // Emit jump noise from the HSM when the jump is actually executed.
            if (ctx.transform != null)
            {
                _noiseSystem.EmitNoise(ctx.transform.position, ctx.stats.JumpNoiseRadius, ctx.transform.gameObject, NoiseType.Jump);
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

        void HandleFootsteps(float deltaTime)
        {
            if (ctx.transform == null || ctx.stats == null)
            {
                _footstepTimer = 0f;
                return;
            }

            if (ctx.IsLandingRollActive)
            {
                _footstepTimer = 0f;
                return;
            }

            if (!ctx.grounded || ctx.currentNoiseRadius <= 0f || ctx.currentFootstepInterval <= 0f)
            {
                _footstepTimer = 0f;
                return;
            }

            if (Mathf.Abs(ctx.input.Move.x) < ctx.stats.NoiseMoveThreshold)
            {
                _footstepTimer = 0f;
                return;
            }

            _footstepTimer += deltaTime;
            if (_footstepTimer < ctx.currentFootstepInterval) return;

            _footstepTimer = 0f;
            _noiseSystem.EmitNoise(ctx.transform.position, ctx.currentNoiseRadius, ctx.transform.gameObject, NoiseType.Footstep);
        }
    }
}

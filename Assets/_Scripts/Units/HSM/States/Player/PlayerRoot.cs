using UnityEngine;

namespace HSM {
    public class PlayerRoot : State {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;
        public readonly Interaction Interaction;
        public readonly Climb Climb;
        public readonly Death Death;
        public readonly PlayerGrabbed Grabbed;
        public readonly LedgeClimb LedgeClimb;
        public readonly InDarkness InDarkness;
        public readonly OnSpikes OnSpikes;
        public readonly Stumble Stumble;

        readonly PlayerContext ctx;
        private float _footstepTimer;

        private NoiseSystem _noiseSystem;


        public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m, null) {
            this.ctx = ctx;
            Grounded    = new Grounded(m, this, ctx);
            Airborne    = new Airborne(m, this, ctx);
            Interaction = new Interaction(m, this, ctx);
            Climb       = new Climb(m, this, ctx);
            Death       = new Death(m, this, ctx);
            Grabbed     = new PlayerGrabbed(m, this, ctx);
            LedgeClimb  = new LedgeClimb(m, this, ctx);
            InDarkness  = new InDarkness(m, this, ctx);
            OnSpikes    = new OnSpikes(m, this, ctx);
            Stumble     = new Stumble(m, this, ctx);

            _noiseSystem = Services.Get<NoiseSystem>();
        }
        
        protected override State GetInitialState() => Grounded;
        protected override State GetTransition()
        {
            if (!ctx.isAlive)       return Machine?.GetState<Death>();
            if (ctx.isGrabbed)      return Machine?.GetState<PlayerGrabbed>();
            if (ctx.OnClimbable) return null;
            if (ctx.isLedgeGrabbing) return null;
            if (ctx.isOnSpikes   && ctx.grounded) return Machine?.GetState<OnSpikes>();
            if (ctx.isInDarkness && ctx.grounded) return Machine?.GetState<InDarkness>();
            if (!ctx.grounded && Time.time >= ctx.frameLeftGrounded + Time.fixedDeltaTime) return Machine?.GetState<Airborne>();
            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!ctx.isAlive)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            // При возврате на землю во время хромания — восстанавливаем нужную фазу
            if (ctx.isStumbling && ctx.grounded && ActiveChild != Stumble)
            {
                if (ctx.limpTimeRemaining > 0f)
                    Machine?.Sequencer.RequestTransition(this, Machine.GetState<StumbleLimping>());
                else
                    ctx.stumblePending = true;
            }

            // Вызываем до base.OnUpdate, чтобы не пропустить из-за IsTransitioning при разгоне
            if (ctx.stumblePending && !ctx.isGrabbed && Machine != null)
                Machine.Sequencer.RequestTransition(this, Machine.GetState<Stumble>());

            if (ctx.stats != null)
            {
                HandleJump();
                HandleFootsteps(deltaTime);
            }
            base.OnUpdate(deltaTime);
        }

        void HandleJump()
        {
            if (ctx.isSceneEntry)
            {
                ctx.jumpToConsume = false;
                return;
            }

            if (ctx.isHiding)
            {
                ctx.jumpToConsume = false;
                return;
            }

            if (ctx.isStumbleFalling || ctx.stumblePending)
            {
                ctx.jumpToConsume = false;
                return;
            }

            if (ctx.isLedgeGrabbing)
                return;

            if (ctx.isClimbing)
                return;

            if (!ctx.endedJumpEarly && !ctx.grounded && !ctx.input.JumpHeld && ctx.velocity.y > 0) ctx.endedJumpEarly = true;
            
            if (ctx.ceilingAbove && ctx.isCrouching)
            {
                ctx.jumpToConsume = false;
                return;
            }

            if (!ctx.jumpToConsume && !ctx.HasBufferedJump) return;

            if ((ctx.grounded || ctx.CanUseCoyote) && ctx.CanJump) ExecuteJump();

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
            ctx.stamina = Mathf.Max(0f, ctx.stamina - ctx.stats.JumpStaminaCost);

            // Emit jump noise from the HSM when the jump is actually executed.
            if (ctx.transform != null)
            {
                _noiseSystem.EmitNoise(ctx.transform.position, ctx.noiseStats.JumpStartNoiseRadius, ctx.transform.gameObject, NoiseType.Jump, ctx.noiseStats.RadiusVariance);
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

            if (Mathf.Abs(ctx.input.Move.x) < ctx.noiseStats.NoiseMoveThreshold)
            {
                _footstepTimer = 0f;
                return;
            }

            _footstepTimer += deltaTime;
            if (_footstepTimer < ctx.currentFootstepInterval) return;

            _footstepTimer = 0f;
            _noiseSystem.EmitNoise(ctx.transform.position, ctx.currentNoiseRadius, ctx.transform.gameObject, NoiseType.Footstep, ctx.noiseStats.RadiusVariance);
        }
    }
}

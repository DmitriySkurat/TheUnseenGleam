using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace HSM {
    public class Run : State {
        readonly PlayerContext ctx;
        public Run(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            //Add(new AnimatorBoolActivity(ctx.anim, "Run", true, false));
            //Add(new AudioLoopActivity(ctx.audio));
            
            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.Run));
        }
        
        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.RunSpeedMultiplier;
            ctx.currentNoiseRadius = ctx.noiseStats.RunNoiseRadius;
            ctx.currentFootstepInterval = ctx.noiseStats.RunFootstepInterval;
            ctx.currentStaminaDrainMultiplier = ctx.stats.RunStaminaDrainMultiplier;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
            ctx.timeRunStarted = Time.time;
            
            base.OnEnter();
        }

        protected override void OnExit()
        {
            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
            base.OnExit();
        }

        protected override State GetTransition() {
            if (!ctx.input.RunHeld || ctx.stamina <= 0.01f) return Machine != null ? Machine.GetState<Walk>() : null;
            
            return null;
        }
    }
}

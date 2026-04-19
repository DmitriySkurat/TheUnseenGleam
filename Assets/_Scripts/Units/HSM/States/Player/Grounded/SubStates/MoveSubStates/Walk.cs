using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace HSM {
    public class Walk : State {
        readonly PlayerContext ctx;
        public Walk(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            //Add(new AnimatorBoolActivity(ctx.anim, "Walk", true, false));
            //Add(new AudioLoopActivity(ctx.audio));
            
            Add(new AnimatorPlayActivity(ctx.anim, "Walk"));
        }
        
        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.WalkSpeedMultiplier;
            ctx.currentNoiseRadius = ctx.noiseStats.WalkNoiseRadius;
            ctx.currentFootstepInterval = ctx.noiseStats.WalkFootstepInterval;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
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
            if (ctx.input.RunHeld && ctx.CanRun) return Machine != null ? Machine.GetState<Run>() : null;
            
            return null;
        }

    }
}

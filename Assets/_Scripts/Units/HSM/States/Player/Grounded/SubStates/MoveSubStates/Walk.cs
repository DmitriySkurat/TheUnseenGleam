using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace HSM {
    public class Walk : State {
        readonly PlayerContext ctx;
        public Walk(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Walking", true, false));
            //Add(new AudioLoopActivity(ctx.audio));
        }
        
        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.WalkSpeedMultiplier;
            ctx.currentNoiseRadius = ctx.stats.WalkNoiseRadius;
            ctx.currentFootstepInterval = ctx.stats.WalkFootstepInterval;
            ctx.currentBreathConsumeMultiplier = ctx.stats.WalkBreathConsumeMultiplier;
            base.OnEnter();
        }

        protected override void OnExit()
        {
            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentBreathConsumeMultiplier = 1f;
            base.OnExit();
        }

        protected override State GetTransition() {
            if (ctx.input.RunHeld && ctx.CanRun) return Machine != null ? Machine.GetState<Run>() : null;
            
            return null;
        }

    }
}

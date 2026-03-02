using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace HSM {
    public class Walk : State {
        readonly PlayerContext ctx;

        public Walk(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Running", true, false));
            Add(new AudioLoopActivity(ctx.audio));
        }
        
        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.WalkSpeedMultiplier;
            base.OnEnter();
        }

        protected override State GetTransition() {
            if (ctx.input.RunHeld && ctx.CanRun) return Machine != null ? Machine.GetState<Run>() : null;
            
            return null;
        }

    }
}

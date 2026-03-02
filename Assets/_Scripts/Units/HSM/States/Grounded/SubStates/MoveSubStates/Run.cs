using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace HSM {
    public class Run : State {
        readonly PlayerContext ctx;

        public Run(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Running", true, false));
            //Add(new AudioLoopActivity(ctx.audio));
        }
        
        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.RunSpeedMultiplier;
            base.OnEnter();
        }

        protected override State GetTransition() {
            if (!ctx.input.RunHeld || ctx.stamina <= 0.01f) return Machine != null ? Machine.GetState<Walk>() : null;
            
            return null;
        }
        
        protected override void OnUpdate(float deltaTime) {        
            ctx.stamina -= ctx.stats.StaminaDrainPerSecond * deltaTime;
            ctx.stamina = Mathf.Max(0, ctx.stamina);
            
            base.OnUpdate(deltaTime);
        }
    }
}

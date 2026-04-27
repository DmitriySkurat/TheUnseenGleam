using System.Buffers.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace HSM {
    public class Move : State {
        readonly PlayerContext ctx;
        
        public readonly Walk Walk;
        public readonly Run Run;

        public Move(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            
            Walk = new Walk(m, this, ctx);
            Run = new Run (m, this, ctx);
            
            //Add(new AudioLoopActivity(ctx.audio));
        }

        protected override State GetInitialState() => Walk;
        
        protected override State GetTransition() {
            if (ctx.isHiding) return Machine != null ? Machine.GetState<Hide>() : null;
            if (ctx.WantsCrouch) return Machine != null ? Machine.GetState<Crouch>() : null;
            //if (Mathf.Abs(ctx.input.Move.x) <= 0.01f) return Machine != null ? Machine.GetState<Idle>() : null;      
            if (!ctx.HasMovementIntent) {
                bool isRunSpeed = Mathf.Abs(ctx.velocity.x) > ctx.stats.MaxSpeed * ctx.stats.WalkSpeedMultiplier;
                if (isRunSpeed) return Machine != null ? Machine.GetState<Stopping>() : null;
                return Machine != null ? Machine.GetState<Idle>() : null;
            }
            
            return null;
        }
    }
}

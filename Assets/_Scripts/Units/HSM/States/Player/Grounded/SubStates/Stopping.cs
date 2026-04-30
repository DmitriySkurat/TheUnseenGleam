using UnityEngine;

namespace HSM {
    public class Stopping : State {
        readonly PlayerContext ctx;

        public Stopping(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.Stop));
        }

        protected override State GetTransition() {
            if (!IsStopAnimationFinished()) return null;

            if (ctx.HasMovementIntent) return Machine != null ? Machine.GetState<Move>() : null;
            if (ctx.isHiding) return Machine != null ? Machine.GetState<Hide>() : null;
            if (ctx.WantsCrouch) return Machine != null ? Machine.GetState<Crouch>() : null;
            if (Mathf.Abs(ctx.velocity.x) < 0.5f) return Machine != null ? Machine.GetState<Idle>() : null;

            return null;
        }

        bool IsStopAnimationFinished() {
            if (ctx.anim == null) return true;
            var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
            if (info.shortNameHash != PlayerAnimations.Stop) return false;
            return info.normalizedTime >= 1f;
        }
    }
}

using UnityEngine;

namespace HSM {
    public class Idle : State {
        readonly PlayerContext ctx;

        public Idle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            //Add(new AnimatorBoolActivity(ctx.anim, "Idle", true, false));

            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.Idle));
        }

        protected override void OnEnter()
        {
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = ctx.stats.IdleStaminaBreathDrainMultiplier;
            base.OnEnter();
        }

        protected override void OnExit()
        {
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            base.OnExit();
        }

        protected override State GetTransition() {
            if (ctx.isHiding) return Machine != null ? Machine.GetState<Hide>() : null;
            if (ctx.WantsCrouch) return Machine != null ? Machine.GetState<Crouch>() : null;
            if (ctx.HasMovementIntent) return Machine != null ? Machine.GetState<Move>() : null;

            return null;
        }

    }
}

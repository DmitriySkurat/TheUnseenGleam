using UnityEngine;

namespace HSM {
    public class Roll : State
    {
        readonly PlayerContext ctx;

        public Roll(StateMachine m, State parent, PlayerContext ctx)
            : base(m, parent)
        {
            this.ctx = ctx;

            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.Roll));
        }

        protected override void OnEnter()
        {
            ctx.isRolling = true;

            base.OnEnter();
        }

        protected override void OnUpdate(float dt)
        {
            ctx.velocity.x =
                ctx.landingRollDirection *
                ctx.stats.LandingRollSpeed;
        }

        protected override void OnExit()
        {
            ctx.isRolling = false;

            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (Time.time >= ctx.landingRollEndTime)
            {
                if (ctx.WantsCrouch)
                    return Machine.GetState<Crouch>();

                if (ctx.HasMovementIntent)
                    return Machine.GetState<Move>();

                return Machine.GetState<Idle>();
            }

            return null;
        }
    }
}
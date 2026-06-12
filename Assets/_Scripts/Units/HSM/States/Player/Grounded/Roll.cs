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
            ctx.velocity.x =
                ctx.landingRollDirection *
                ctx.stats.LandingRollSpeed;
        
            ctx.isRolling = true;

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {            
            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.isRolling = false;

            base.OnExit();
        }

        protected override State GetTransition()
        {
            
            Debug.Log(
    $"now={Time.time} end={ctx.landingRollEndTime}"
);
    
            if (Time.time >= ctx.landingRollEndTime)
            {
                Debug.Log("ROLL EXIT");
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
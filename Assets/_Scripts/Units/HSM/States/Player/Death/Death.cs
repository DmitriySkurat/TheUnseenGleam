using UnityEngine;

namespace HSM
{
    public class Death : State
    {
        private PlayerContext ctx;

        public Death(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.black,
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Climb", true, false));
        }

        protected override void OnEnter()
        {
            ctx.velocity = Vector2.zero;
        }
    }
}
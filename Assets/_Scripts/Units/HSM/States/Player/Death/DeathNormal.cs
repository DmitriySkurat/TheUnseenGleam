using UnityEngine;

namespace HSM
{
    public class DeathNormal : State
    {
        private PlayerContext ctx;

        public DeathNormal(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Death", true, false));
        }

        protected override void OnEnter()
        {
            Debug.Log("Player has died (normal). Ending session...");
            Services.Get<SessionEndHandler>().EndSession();
            base.OnEnter();
        }
    }
}

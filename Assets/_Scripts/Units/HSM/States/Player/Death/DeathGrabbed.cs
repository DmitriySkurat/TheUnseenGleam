using UnityEngine;

namespace HSM
{
    public class DeathGrabbed : State
    {
        private PlayerContext ctx;

        public DeathGrabbed(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "DeathGrabbed", true, false));
        }

        protected override void OnEnter()
        {
            Debug.Log("Player has died while grabbed. Ending session...");
            Services.Get<SessionEndHandler>().EndSession();
            base.OnEnter();
        }
    }
}

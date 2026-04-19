using UnityEngine;


namespace HSM
{
    public class Interaction : State
    {
        readonly PlayerContext ctx;
        
        private IInteractable _interactable;
        
        public void SetInteractable(IInteractable interactable) => _interactable = interactable;
        
        protected override State GetTransition() => ctx.isInteracting ? null : (Machine != null ? Machine.GetState<Grounded>() : null);
        
        public Interaction(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            
            Add(new AnimatorBoolActivity(ctx.anim, "Interact", true, false));
        }
    }
    
}
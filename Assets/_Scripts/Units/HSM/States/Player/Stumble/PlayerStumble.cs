using UnityEngine;

namespace HSM {
    public class Stumble : State
    {
        public readonly StumbleFalling Falling;
        public readonly StumbleLimping Limping;

        readonly PlayerContext ctx;

        public Stumble(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Falling = new StumbleFalling(m, this, ctx);
            Limping = new StumbleLimping(m, this, ctx);
        }

        protected override State GetInitialState() => Falling;

        protected override void OnEnter()
        {
            ctx.stumblePending = false;
            ctx.jumpToConsume  = false;
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.jumpToConsume = false;
            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.currentSpeedMultiplier = 1f;
            base.OnExit();
        }

        // Выход только через PlayerRoot: isGrabbed → PlayerGrabbed, !isAlive → Death
        protected override State GetTransition() => null;
    }
}

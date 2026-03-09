using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyChase : State
    {
        private readonly EnemyContext ctx;
        private Transform lastTarget;

        public EnemyChase(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            if (ctx?.pathfinder == null) return;
            lastTarget = ctx.target;
            if (ctx.target != null) ctx.pathfinder.SetTarget(ctx.target);
            ctx.pathfinder.tryFindPath = ctx.target != null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx?.pathfinder != null)
            {
                if (ctx.target != null && !ReferenceEquals(ctx.target, lastTarget))
                {
                    lastTarget = ctx.target;
                    ctx.pathfinder.SetTarget(ctx.target);
                }

                if (ctx.target == null && ctx.pathfinder.tryFindPath)
                {
                    ctx.pathfinder.tryFindPath = false;
                }
                else if (ctx.target != null && !ctx.pathfinder.tryFindPath)
                {
                    ctx.pathfinder.tryFindPath = true;
                }
            }
            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            if (ctx?.pathfinder != null) ctx.pathfinder.tryFindPath = false;
        }

        protected override State GetTransition()
        {
            if (ctx != null && ctx.ShouldStopChase) return Machine != null ? Machine.GetState<EnemyIdle>() : null;
            return null;
        }
    }
}

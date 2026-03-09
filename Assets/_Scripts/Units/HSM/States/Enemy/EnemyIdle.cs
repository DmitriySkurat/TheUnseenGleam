using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyIdle : State
    {
        private readonly EnemyContext ctx;

        public EnemyIdle(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            if (ctx?.pathfinder == null) return;
            ctx.pathfinder.tryFindPath = false;
            ctx.pathfinder.SetVelocity(Vector2.zero);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx?.pathfinder != null)
            {
                if (ctx.pathfinder.tryFindPath) ctx.pathfinder.tryFindPath = false;
                ctx.pathfinder.SetVelocity(Vector2.zero);
            }
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (CanChase()) return Machine != null ? Machine.GetState<EnemyChase>() : null;
            if (ctx != null && ctx.hasNoiseTarget) return Machine != null ? Machine.GetState<EnemyInvestigate>() : null;
            return null;
        }

        private bool CanChase()
        {
            if (ctx == null || ctx.target == null || ctx.self == null) return false;
            float distance = Vector3.Distance(ctx.self.position, ctx.target.position);
            return distance <= ctx.detectRange;
        }
    }
}

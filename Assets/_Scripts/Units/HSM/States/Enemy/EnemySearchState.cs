using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemySearchState : State
    {
        private readonly EnemyContext ctx;
        private float _startTime;
        private Vector2 _currentPoint;
        private bool _hasPoint;

        public EnemySearchState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _startTime = Time.time;
            _hasPoint = false;
            PickNextPoint();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            if (!_hasPoint && ctx.hasLastKnownPlayerPosition)
            {
                PickNextPoint();
            }
            else if (_hasPoint && ctx.self != null)
            {
                float threshold = ctx.searchPointReachedDistance > 0f ? ctx.searchPointReachedDistance : 0.25f;
                if (Vector2.Distance(ctx.self.position, _currentPoint) <= threshold)
                {
                    PickNextPoint();
                }
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx?.movement?.Stop();
        }

        protected override State GetTransition()
        {
            if (ctx == null) return null;
            if (ctx.vision != null && ctx.vision.CanSeePlayer) return Machine != null ? Machine.GetState<EnemyChaseState>() : null;
            if (ctx.hasNoiseTarget) return Machine != null ? Machine.GetState<EnemyInvestigateState>() : null;
            if (!ctx.hasLastKnownPlayerPosition) return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            if (ctx.searchDuration > 0f && Time.time - _startTime >= ctx.searchDuration)
                return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            return null;
        }

        private void PickNextPoint()
        {
            if (ctx == null || !ctx.hasLastKnownPlayerPosition)
            {
                _hasPoint = false;
                return;
            }

            Vector2 center = ctx.lastKnownPlayerPosition;
            float radius = ctx.searchRadius > 0f ? ctx.searchRadius : 0f;
            Vector2 offset = radius > 0f ? UnityEngine.Random.insideUnitCircle * radius : Vector2.zero;
            _currentPoint = center + offset;
            _hasPoint = true;
            ctx.movement?.MoveTo(_currentPoint);
        }
    }
}

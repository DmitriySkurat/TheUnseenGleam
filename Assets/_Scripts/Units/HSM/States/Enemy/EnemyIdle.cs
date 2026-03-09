using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyPatrolState : State
    {
        private readonly EnemyContext ctx;
        private int _index;
        private float _waitTimer;
        private bool _waiting;
        private Vector2 _currentPoint;

        public EnemyPatrolState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _waiting = false;
            _waitTimer = 0f;
            _index = ctx != null ? ctx.patrolIndex : 0;
            MoveToCurrentPoint();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            if (_waiting)
            {
                _waitTimer -= deltaTime;
                if (_waitTimer <= 0f)
                {
                    _waiting = false;
                    AdvancePoint();
                }
            }
            else if (HasPatrolPoints() && IsAtPoint(_currentPoint))
            {
                _waiting = true;
                _waitTimer = ctx.patrolWaitTime;
                ctx.movement?.Stop();
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (CanSeePlayer()) return Machine != null ? Machine.GetState<EnemyChaseState>() : null;
            if (ctx != null && ctx.hasNoiseTarget) return Machine != null ? Machine.GetState<EnemyInvestigateState>() : null;
            return null;
        }

        private bool CanSeePlayer()
        {
            if (ctx == null || ctx.vision == null) return false;
            return ctx.vision.CanSeePlayer;
        }

        private bool HasPatrolPoints()
        {
            return ctx != null && ctx.patrolPoints != null && ctx.patrolPoints.Length > 0;
        }

        private void MoveToCurrentPoint()
        {
            if (!HasPatrolPoints())
            {
                ctx?.movement?.Stop();
                return;
            }

            int attempts = 0;
            while (attempts < ctx.patrolPoints.Length)
            {
                if (_index < 0 || _index >= ctx.patrolPoints.Length) _index = 0;
                Transform point = ctx.patrolPoints[_index];
                if (point != null)
                {
                    _currentPoint = point.position;
                    ctx.patrolIndex = _index;
                    ctx.movement?.MoveTo(_currentPoint);
                    return;
                }

                _index = (_index + 1) % ctx.patrolPoints.Length;
                attempts++;
            }

            ctx.movement?.Stop();
        }

        private void AdvancePoint()
        {
            if (!HasPatrolPoints()) return;
            _index = (_index + 1) % ctx.patrolPoints.Length;
            MoveToCurrentPoint();
        }

        private bool IsAtPoint(Vector2 point)
        {
            if (ctx == null || ctx.self == null) return false;
            float threshold = ctx.patrolPointReachedDistance > 0f ? ctx.patrolPointReachedDistance : 0.25f;
            return Vector2.Distance(ctx.self.position, point) <= threshold;
        }
    }
}

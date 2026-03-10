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
        private bool _fallback;
        private bool _fallbackCached;
        private Vector2 _fallbackPointA;
        private Vector2 _fallbackPointB;

        public EnemyPatrolState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _waiting = false;
            _waitTimer = 0f;
            _index = ctx != null ? ctx.patrolIndex : 0;
            _fallback = false;
            if (!HasPatrolPoints())
            {
                _index = 0;
            }
            MoveToCurrentPoint();
            if (ctx != null && ctx.debugStateLifecycle) ctx.Log("Patrol: Enter");
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
            else if ((HasPatrolPoints() || _fallback) && IsAtPoint(_currentPoint))
            {
                _waiting = true;
                _waitTimer = ctx.patrolWaitTime;
                ctx.movement?.Stop();
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            if (ctx != null && ctx.debugStateLifecycle) ctx.Log("Patrol: Exit");
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (CanSeePlayer())
            {
                if (ctx != null && ctx.debugTransitions) ctx.Log("Patrol -> Chase (vision)");
                return Machine != null ? Machine.GetState<EnemyChaseState>() : null;
            }
            if (ctx != null && ctx.hasNoiseTarget)
            {
                if (ctx.debugTransitions) ctx.Log("Patrol -> Investigate (noise)");
                return Machine != null ? Machine.GetState<EnemyInvestigateState>() : null;
            }
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
                MoveToFallbackPoint();
                return;
            }

            int attempts = 0;
            while (attempts < ctx.patrolPoints.Length)
            {
                if (_index < 0 || _index >= ctx.patrolPoints.Length) _index = 0;
                Transform point = ctx.patrolPoints[_index];
                if (point != null)
                {
                    Vector2 desired = point.position;
                    Vector2 resolved = desired;
                    if (ctx.movement is EnemyMotor motor)
                    {
                        motor.TryResolveTarget(desired, out resolved);
                    }
                    _currentPoint = resolved;
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
            if (!HasPatrolPoints())
            {
                _index = 1 - _index;
                MoveToFallbackPoint();
                return;
            }
            _index = (_index + 1) % ctx.patrolPoints.Length;
            MoveToCurrentPoint();
        }

        private bool IsAtPoint(Vector2 point)
        {
            if (ctx == null || ctx.self == null) return false;
            float threshold = ctx.patrolPointReachedDistance > 0f ? ctx.patrolPointReachedDistance : 0.25f;
            return Vector2.Distance(ctx.self.position, point) <= threshold;
        }

        private void MoveToFallbackPoint()
        {
            if (ctx == null)
            {
                return;
            }
            _fallback = true;
            if (!_fallbackCached)
            {
                _fallbackCached = BuildFallbackPoints();
                _index = 0;
            }
            if (!_fallbackCached)
            {
                ctx.movement?.Stop();
                return;
            }

            if (_index != 0 && _index != 1)
            {
                _index = 0;
            }
            _currentPoint = _index == 0 ? _fallbackPointA : _fallbackPointB;
            ctx.movement?.MoveTo(_currentPoint);
        }

        private bool BuildFallbackPoints()
        {
            if (ctx == null) return false;
            float radius = ctx.fallbackPatrolRadius > 0f ? ctx.fallbackPatrolRadius : 0f;
            if (radius <= 0f) return false;

            Vector2 center = ctx.spawnPosition;
            Vector2 desiredA = center + Vector2.right * radius;
            Vector2 desiredB = center + Vector2.right * -radius;
            Vector2 resolvedA = desiredA;
            Vector2 resolvedB = desiredB;
            bool foundA = false;
            bool foundB = false;

            if (ctx.pathfinder != null)
            {
                int steps = ctx.pathfinder.StepsForDistance(radius);
                foundA = ctx.pathfinder.TryGetWalkableInDirection(center, 1, steps, out resolvedA);
                foundB = ctx.pathfinder.TryGetWalkableInDirection(center, -1, steps, out resolvedB);
                if ((!foundA || !foundB) && radius > 0f)
                {
                    int moreSteps = Mathf.Max(steps + 1, ctx.pathfinder.StepsForDistance(radius * 2f));
                    if (!foundA) foundA = ctx.pathfinder.TryGetWalkableInDirection(center, 1, moreSteps, out resolvedA);
                    if (!foundB) foundB = ctx.pathfinder.TryGetWalkableInDirection(center, -1, moreSteps, out resolvedB);
                }
            }

            if (ctx.movement is EnemyMotor motor)
            {
                if (!foundA) motor.TryResolveTarget(desiredA, out resolvedA);
                if (!foundB) motor.TryResolveTarget(desiredB, out resolvedB);
            }

            float minSeparation = Mathf.Max(0.35f, ctx.patrolPointReachedDistance);
            if (Vector2.Distance(resolvedA, resolvedB) < minSeparation)
            {
                Vector2 fartherA = center + Vector2.right * radius * 2f;
                Vector2 fartherB = center + Vector2.right * -radius * 2f;
                if (ctx.movement is EnemyMotor motorFar)
                {
                    motorFar.TryResolveTarget(fartherA, out resolvedA);
                    motorFar.TryResolveTarget(fartherB, out resolvedB);
                }
            }

            _fallbackPointA = resolvedA;
            _fallbackPointB = resolvedB;
            return true;
        }
    }
}

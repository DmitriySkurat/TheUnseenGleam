using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemySearchState : State
    {
        private readonly EnemyContext ctx;
        private float _startTime;
        private Vector2 _currentPoint;
        private Vector2 _anchor;
        private int _pointIndex;
        private Vector2 _pointA;
        private Vector2 _pointB;
        private bool _waiting;
        private float _waitTimer;
        private float _searchDuration;

        public EnemySearchState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _startTime = Time.time;
            _pointIndex = 0;
            _waiting = false;
            _waitTimer = 0f;
            _searchDuration = 0f;
            if (ctx != null)
            {
                _searchDuration = ctx.searchDuration;
                if (ctx.usePostChasePatrolDuration && ctx.postChasePatrolDuration > 0f)
                {
                    _searchDuration = ctx.postChasePatrolDuration;
                }
                ctx.usePostChasePatrolDuration = false;
            }
            _anchor = ctx != null && ctx.hasLastKnownPlayerPosition ? ctx.lastKnownPlayerPosition : Vector2.zero;
            BuildPoints();
            MoveToPoint(_pointIndex);
            if (ctx != null && ctx.debugStateLifecycle) ctx.Log("Search: Enter");
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            if (ctx.hasLastKnownPlayerPosition)
            {
                if (Vector2.Distance(_anchor, ctx.lastKnownPlayerPosition) > 0.25f)
                {
                    _anchor = ctx.lastKnownPlayerPosition;
                    _pointIndex = 0;
                    _waiting = false;
                    _waitTimer = 0f;
                    BuildPoints();
                    MoveToPoint(_pointIndex);
                }
            }
            if (ctx.self != null)
            {
                float threshold = ctx.searchPointReachedDistance > 0f ? ctx.searchPointReachedDistance : 0.25f;
                if (_waiting)
                {
                    _waitTimer -= deltaTime;
                    if (_waitTimer <= 0f)
                    {
                        _waiting = false;
                        _pointIndex = 1 - _pointIndex;
                        MoveToPoint(_pointIndex);
                    }
                }
                else if (Vector2.Distance(ctx.self.position, _currentPoint) <= threshold)
                {
                    _waiting = true;
                    _waitTimer = ctx.patrolWaitTime;
                    ctx.movement?.Stop();
                }
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx?.movement?.Stop();
            if (ctx != null && ctx.debugStateLifecycle) ctx.Log("Search: Exit");
        }

        protected override State GetTransition()
        {
            if (ctx == null) return null;
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                if (ctx.debugTransitions) ctx.Log("Search -> Chase (vision)");
                return Machine != null ? Machine.GetState<EnemyChaseState>() : null;
            }
            if (ctx.hasNoiseTarget)
            {
                if (ctx.debugTransitions) ctx.Log("Search -> Investigate (noise)");
                return Machine != null ? Machine.GetState<EnemyInvestigateState>() : null;
            }
            if (!ctx.hasLastKnownPlayerPosition)
            {
                if (ctx.debugTransitions) ctx.Log("Search -> Patrol (no last known)");
                return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            }
            float effectiveDuration = _searchDuration;
            if (effectiveDuration <= 0f)
            {
                ctx.hasLastKnownPlayerPosition = false;
                if (ctx.debugTransitions) ctx.Log("Search -> Patrol (duration <= 0)");
                return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            }
            if (Time.time - _startTime >= effectiveDuration)
            {
                ctx.hasLastKnownPlayerPosition = false;
                if (ctx.debugTransitions) ctx.Log("Search -> Patrol (timeout)");
                return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            }
            return null;
        }

        private void BuildPoints()
        {
            if (ctx == null || !ctx.hasLastKnownPlayerPosition)
            {
                return;
            }

            Vector2 center = _anchor;
            float radius = ctx.searchRadius > 0f ? ctx.searchRadius : 0.75f;
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

            float minSeparation = Mathf.Max(0.35f, ctx.searchPointReachedDistance);
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

            _pointA = resolvedA;
            _pointB = resolvedB;
        }

        private void MoveToPoint(int index)
        {
            Vector2 target = index == 0 ? _pointA : _pointB;
            if (ctx != null && ctx.self != null)
            {
                float threshold = ctx.searchPointReachedDistance > 0f ? ctx.searchPointReachedDistance : 0.25f;
                if (Vector2.Distance(ctx.self.position, target) <= threshold)
                {
                    Vector2 alt = index == 0 ? _pointB : _pointA;
                    if (Vector2.Distance(ctx.self.position, alt) > threshold)
                    {
                        target = alt;
                    }
                    else
                    {
                        float step = ctx.pathfinder != null ? ctx.pathfinder.GetCellSizeX() : threshold * 2f;
                        Vector2 desired = _anchor + Vector2.right * step * (index == 0 ? 1f : -1f);
                        if (ctx.movement is EnemyMotor motor)
                        {
                            motor.TryResolveTarget(desired, out target);
                        }
                    }
                }
            }

            _currentPoint = target;
            ctx?.movement?.MoveTo(_currentPoint);
        }
    }
}

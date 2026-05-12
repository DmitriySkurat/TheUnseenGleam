using PlatNav;
using UnityEngine;

namespace HSM {
    /// <summary>
    /// Листовое подсостояние AgentPatrol: навигация по точкам маршрута.
    /// По истечении случайного таймера переходит в AgentLookAround.
    /// Внешние триггеры (игрок, шум, тревога) обрабатываются родителем AgentPatrol.
    /// </summary>
    public class AgentPatrolWalk : State
    {
        readonly AgentContext ctx;
        bool _navigating;
        float _lookAroundTimer;
        bool _triggerLookAround;
        bool _wasTraversing;

        public AgentPatrolWalk(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _wasTraversing = false;
            ctx.anim?.Play(AgentAnimations.Walk, 0, 0f);

            ctx.nav.SetTarget(null);
            ctx.nav.Abort();

            EnsurePatrolPoints();
            ctx.patrolWaitTimer = 0f;
            _navigating        = false;
            _triggerLookAround = false;
            _lookAroundTimer   = Random.Range(
                ctx.stats.LookAroundMinInterval,
                ctx.stats.LookAroundMaxInterval);

            StartNavigatingToCurrentPoint();
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            bool traversing = ctx.IsTraversingLink;
            if (traversing != _wasTraversing)
            {
                var traversalAnim = ctx.nav.IsTraversingFall ? AgentAnimations.Dropdown : AgentAnimations.Jump;
                ctx.anim?.Play(traversing ? traversalAnim : AgentAnimations.Walk, 0, 0f);
                _wasTraversing = traversing;
            }

            if (ctx.IsWaitingAtPoint)
            {
                ctx.patrolWaitTimer -= deltaTime;
                if (!ctx.IsWaitingAtPoint)
                    AdvanceToNextPoint();
            }
            else if (_navigating)
            {
                ctx.nav.SetSpeed(ctx.isBlindedByEnvironment
                    ? ctx.stats.BlindedByEnvironmentSpeed
                    : ctx.stats.PatrolSpeed);
                ctx.nav.Tick(deltaTime);
                if (ctx.nav.State == PlatNavState.Idle)
                {
                    _navigating = false;
                    ctx.patrolWaitTimer = ctx.stats.PatrolWaitTime;
                }
            }
            else
            {
                // Path failed on previous attempt — retry
                StartNavigatingToCurrentPoint();
            }

            _lookAroundTimer -= deltaTime;
            if (_lookAroundTimer <= 0f)
                _triggerLookAround = true;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (_triggerLookAround && !ctx.IsWaitingAtPoint && !ctx.IsTraversingLink
                && !ctx.isBlindedByEnvironment && !ctx.isBlindedByPlayer)
            {
                _triggerLookAround = false;
                return Machine?.GetState<AgentLookAround>();
            }
            return null;
        }

        void EnsurePatrolPoints()
        {
            if (ctx.PatrolCount > 0) return;

            float dist = ctx.stats != null ? ctx.stats.DefaultPatrolDistance : 4f;
            ctx.patrolPositions = new Vector2[]
            {
                ctx.spawnPosition + Vector2.left  * dist,
                ctx.spawnPosition + Vector2.right * dist,
            };
        }

        void StartNavigatingToCurrentPoint()
        {
            if (ctx.PatrolCount == 0) return;
            _navigating = ctx.nav.MoveTo(ctx.CurrentPatrolPosition, ctx.stats.PatrolSpeed);
        }

        void AdvanceToNextPoint()
        {
            ctx.currentPatrolIndex = (ctx.currentPatrolIndex + 1) % ctx.PatrolCount;
            StartNavigatingToCurrentPoint();
        }
    }
}

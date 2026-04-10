using UnityEngine;

namespace HSM {
    public class AgentGrabPlayer : State
    {
        readonly AgentContext ctx;

        bool _isHolding;

        public AgentGrabPlayer(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.Abort();
            // Первый захват в этой погоне — полная задержка;
            // повторный — без задержки, паузу уже отыграл GrabCooldown в AgentChase
            ctx.attackFirstHitTimer = ctx.grabOccurredInChase
                ? 0f
                : ctx.stats.AttackFirstHitDelay;
            _isHolding = false;
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!_isHolding)
            {
                ctx.attackFirstHitTimer -= deltaTime;
                if (ctx.attackFirstHitTimer <= 0f)
                    StartGrab();
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            // Гарантированно снимаем захват при любом выходе из состояния
            if (ctx.playerCtx != null)
                ctx.playerCtx.isGrabbed = false;

            // Запускаем кулдаун, чтобы агент не мог сразу схватить снова
            ctx.grabCooldownTimer = ctx.stats.GrabCooldown;
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Игрок вырвался — преследуем
            if (_isHolding && ctx.playerCtx != null && !ctx.playerCtx.isGrabbed)
                return Machine?.GetState<AgentChase>();

            // Игрок вышел из зоны до того, как агент успел схватить
            if (!_isHolding)
            {
                float dist = Vector2.Distance(ctx.transform.position, ctx.playerTransform.position);
                if (dist > ctx.stats.AttackRange)
                    return Machine?.GetState<AgentChase>();
            }

            return null;
        }

        void StartGrab()
        {
            if (ctx.playerCtx == null) return;
            ctx.playerCtx.isGrabbed = true;
            ctx.playerCtx.grabEscapeCount = ctx.stats.GrabEscapeCount;
            ctx.grabOccurredInChase = true; // следующий захват в этой погоне будет повторным
            _isHolding = true;
        }
    }
}

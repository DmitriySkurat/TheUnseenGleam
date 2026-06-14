using PlatNav;
using UnityEngine;

namespace HSM {
    public class AgentGrabPlayer : State
    {
        readonly AgentContext ctx;

        bool _isHolding;
        bool _isDragging;

        public AgentGrabPlayer(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            if (ctx.anim != null)
                Add(new AnimatorPlayActivity(ctx.anim, AgentAnimations.GrabPlayer));
        }

        protected override void OnEnter()
        {
            ctx.nav.Abort();

            bool ledgeGrab =
                ctx.playerCtx != null &&
                ctx.playerCtx.isLedgeGrabbing;

            ctx.grabFirstHitTimer =
                (ctx.grabOccurredInChase || ledgeGrab)
                    ? 0f
                    : ctx.stats.AttackFirstHitDelay;

            _isHolding = false;
            _isDragging = false;

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!_isHolding)
            {
                ctx.grabFirstHitTimer -= deltaTime;
                if (ctx.grabFirstHitTimer <= 0f)
                    StartGrab();
            }
            else if (ctx.playerCtx != null && !ctx.playerCtx.isAlive)
            {
                if (!_isDragging)
                    StartDragging();

                ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
                ctx.nav.Tick(deltaTime);

                // Если дошли до целевой точки — назначаем новую ещё дальше
                if (ctx.nav.State == PlatNavState.Idle)
                    StartDragging();

                KeepPlayerAttached();
            }
            else
            {
                KeepPlayerAttached();
            }

            base.OnUpdate(deltaTime);
        }

        void StartDragging()
        {
            _isDragging = true;
            
            ctx.anim?.Play(AgentAnimations.DeathGrabbed, 0, 0f);
            
            ctx.nav.SetTarget(null);
            float facingDir = ctx.transform.localScale.x >= 0f ? 1f : -1f;
            Vector2 farPoint = (Vector2)ctx.transform.position + Vector2.right * (facingDir * 60f);
            ctx.nav.MoveTo(farPoint, ctx.stats.ChaseSpeed);
        }

        void KeepPlayerAttached()
        {
            float facingDir = ctx.transform.localScale.x >= 0f ? 1f : -1f;
            Vector2 grabPos = (Vector2)ctx.transform.position + Vector2.right * (facingDir * ctx.stats.GrabPlayerOffset);
            ctx.playerRb.position = grabPos;
            ctx.playerCtx.velocity = Vector2.zero;
        }

        protected override void OnExit()
        {
            if (_isDragging)
                ctx.nav.Abort();

            ctx.isGrabbingPlayer = false;

            // Гарантированно снимаем захват при любом выходе из состояния
            if (ctx.playerCtx != null)
                ctx.playerCtx.isGrabbed = false;
                
                Debug.Log("UNGRAB FROM " + GetType().Name);

            // Запускаем кулдаун, чтобы агент не мог сразу схватить снова
            ctx.grabCooldownTimer = ctx.stats.GrabCooldown;
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Игрок мёртв — агент уносит тело, не преследует
            if (ctx.playerCtx != null && !ctx.playerCtx.isAlive)
                return null;

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
            
            Debug.Log($"START GRAB ledge={ctx.playerCtx.isLedgeGrabbing}");

            // Сбрасываем скорость игрока, иначе инерция сдвинет его с позиции захвата
            // до того, как PlayerGrabbed.OnUpdate успеет обнулить ctx.velocity
            ctx.playerCtx.velocity = Vector2.zero;
            ctx.playerRb.linearVelocity = Vector2.zero;

            // Переместить игрока перед агентом
            float facingDir = ctx.transform.localScale.x >= 0f ? 1f : -1f;
            Vector2 grabPos = (Vector2)ctx.transform.position + Vector2.right * (facingDir * ctx.stats.GrabPlayerOffset);
            ctx.playerRb.position = grabPos;
Debug.Log($"GrabPos={grabPos} PlayerPos={ctx.playerRb.position}");
            ctx.isGrabbingPlayer = true;
            ctx.playerCtx.isGrabbed = true;
            if (Services.IsRegistered<DialogManager>())
                Services.Get<DialogManager>().Cancel();
            ctx.playerCtx.grabEscapeCount = ctx.stats.GrabEscapeCount;
            ctx.grabOccurredInChase = true; // следующий захват в этой погоне будет повторным
            _isHolding = true;
        }
    }
}

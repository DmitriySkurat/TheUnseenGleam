using UnityEngine;

namespace HSM {
    public class PlayerGrabbed : State
    {
        readonly PlayerContext ctx;

        int _progress;
        int _lastDir; // -1 = A, +1 = D, 0 = ещё не нажималось
        float _progressFraction; // дробная часть прогресса для плавного убывания

        public PlayerGrabbed(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            
            Add(new AnimatorBoolActivity(ctx.anim, PlayerAnimations.Grabbed, true, false));
        }

        protected override void OnEnter()
        {
            _progress = 0;
            _lastDir  = 0;
            ctx.grabProgress = 0f;
            _progressFraction = 0f;
            ctx.currentStaminaDrainMultiplier = ctx.stats.GrabbedStaminaDrainMultiplier;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Замораживаем игрока
            ctx.velocity       = Vector2.zero;
            ctx.jumpToConsume  = false;

            // Считаем смены направления (лояльный алгоритм)
            // Повторное нажатие той же кнопки не сбрасывает прогресс — просто игнорируется.
            float moveX = ctx.input.Move.x;
            int dir = moveX < -0.1f ? -1 : (moveX > 0.1f ? 1 : 0);

            if (dir != 0 && dir != _lastDir)
            {
                _lastDir = dir;
                _progressFraction += 1f;
            }

            // Постепенное убывание прогресса со временем
            if (ctx.grabEscapeCount > 0)
            {
                float drainInSteps = ctx.stats.GrabProgressDrainPerSecond * ctx.grabEscapeCount * deltaTime;
                _progressFraction = Mathf.Max(0f, _progressFraction - drainInSteps);
            }

            _progress = Mathf.FloorToInt(_progressFraction);

            ctx.grabProgress = ctx.grabEscapeCount > 0
                ? Mathf.Clamp01(_progressFraction / ctx.grabEscapeCount)
                : 0f;

            if (!ctx.grabEscapeDisabled && _progressFraction >= ctx.grabEscapeCount)
                ctx.isGrabbed = false; // сигнал агенту

            // Постепенный урон пока игрок схвачен
            ctx.health?.TakeDamage(ctx.stats.GrabDamagePerSecond * deltaTime);

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.grabEscapeDisabled = false;
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (!ctx.isAlive)
            {
                ctx.diedWhileGrabbed = true;
                return Machine?.GetState<Death>();
            }
            if (!ctx.isGrabbed) return Machine?.GetState<Grounded>();
            return null;
        }
    }
}

using UnityEngine;

namespace HSM {
    public class PlayerGrabbed : State
    {
        readonly PlayerContext ctx;

        int _progress;
        int _lastDir; // -1 = A, +1 = D, 0 = ещё не нажималось
        float _timer;

        public PlayerGrabbed(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _progress = 0;
            _lastDir  = 0;
            _timer    = ctx.stats.GrabSessionTimeout;
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
                _progress++;
            }

            if (_progress >= ctx.grabEscapeCount)
                ctx.isGrabbed = false; // сигнал агенту

            _timer -= deltaTime;
            if (_timer <= 0f)
            {
                Debug.Log("Player failed to escape the grab in time.");
                
                Services.Get<SessionEndHandler>().EndSession(0f);
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (!ctx.isAlive)   return Machine?.GetState<Death>();
            if (!ctx.isGrabbed) return Machine?.GetState<Grounded>();
            return null;
        }
    }
}

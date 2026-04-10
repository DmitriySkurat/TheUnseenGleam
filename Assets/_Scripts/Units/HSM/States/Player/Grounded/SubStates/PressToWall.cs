using UnityEngine;

namespace HSM {
    /// <summary>
    /// Игрок прижимается к стене.
    /// Вход: одиночное нажатие W (Move.y > threshold).
    /// Выход: нажатие S (Move.y &lt; -threshold) или попадание на лестницу.
    /// Дыхание задерживать вручную — isHidingInLight сработает автоматически
    /// если игрок удерживает задержку дыхания и находится в свету.
    /// </summary>
    public class PressToWall : State {
        readonly PlayerContext ctx;

        public PressToWall(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "PressToWall", true, false));
        }

        protected override void OnEnter() {
            ctx.isPressedToWall = true;
            ctx.velocity        = Vector2.zero;

            ctx.currentNoiseRadius      = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier           = 0f;
            ctx.currentStaminaBreathDrainMultiplier     = 0f;

            base.OnEnter();
        }

        protected override void OnExit() {
            ctx.isPressedToWall = false;
            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime) {
            ctx.velocity.x = 0f;
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition() {
            // На лестнице — выйти
            if (ctx.OnClimbable)
                return Machine?.GetState<Idle>();

            // S нажат — отойти от стены
            if (ctx.input.Move.y < -ctx.stats.VerticalDeadZoneThreshold)
                return Machine?.GetState<Idle>();

            return null;
        }
    }
}

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

        CapsuleCollider2D col;
        Vector2 originalColliderSize;
        Vector2 originalColliderOffset;
        
        float enterTime;
        
        float timeBlockJump = 0.35f;
        float timeBlockMove = 0.35f;
        

        public PressToWall(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            //Add(new AnimatorBoolActivity(ctx.anim, "PressToWall", true, false));
            
            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.PressToWall));
        }

        protected override void OnEnter() {
            enterTime = Time.time;
            ctx.jumpBlockedUntil = Time.time + timeBlockJump;
        
            ctx.isPressedToWall = true;
            ctx.velocity        = Vector2.zero;

            var p = ctx.transform.position;
            ctx.transform.position = new Vector3(p.x, p.y, 2f);

            ctx.currentNoiseRadius      = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier           = 0f;
            ctx.currentStaminaBreathDrainMultiplier     = ctx.stats.PressToWallStaminaBreathDrainMultiplier;

            col = ctx.coll as CapsuleCollider2D;
            if (col != null) {
                originalColliderSize   = col.size;
                originalColliderOffset = col.offset;

                Vector2 newSize = new Vector2(
                    originalColliderSize.x * ctx.stats.PressToWallWidthMultiplier,
                    originalColliderSize.y * ctx.stats.PressToWallHeightMultiplier
                );
                // сдвигаем оффсет вниз так, чтобы низ коллайдера оставался на месте
                float deltaY = (originalColliderSize.y - newSize.y) * 0.5f;
                col.size   = newSize;
                col.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - deltaY);
            }

            base.OnEnter();
        }

        protected override void OnExit() {
            ctx.isPressedToWall = false;

            var p = ctx.transform.position;
            ctx.transform.position = new Vector3(p.x, p.y, 0f);

            if (col != null) {
                col.size   = originalColliderSize;
                col.offset = originalColliderOffset;
            }

            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime) {
            ctx.velocity.x = 0f;
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition() {
            float timeSinceEnter = Time.time - enterTime;
            
            // Первые N сек запрещаем любое движение
            if (timeSinceEnter < timeBlockMove)
                return null;
            
            // На лестнице — выйти (Climb подхватит родительский Grounded)
            if (ctx.OnClimbable)
                return Machine?.GetState<Idle>();

            // Пока W зажата — выход заблокирован (кроме лестницы выше)
            if (ctx.input.Move.y > ctx.stats.VerticalDeadZoneThreshold)
                return null;

            // S нажат — отойти от стены
            if (ctx.input.Move.y < -ctx.stats.VerticalDeadZoneThreshold)
                return Machine?.GetState<Idle>();

            // W отпущена — выйти в Move или Idle
            if (Mathf.Abs(ctx.input.Move.x) > ctx.stats.HorizontalDeadZoneThreshold)
                return Machine?.GetState<Move>();

            return null;
        }
    }
}

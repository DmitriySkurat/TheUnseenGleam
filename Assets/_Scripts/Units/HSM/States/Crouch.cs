using UnityEngine;

namespace HSM {
    public class Crouch : State {
        readonly PlayerContext ctx;
        
        CapsuleCollider2D col;
        Vector2 originalColliderSize;
        Vector2 originalColliderOffset;

        public Crouch(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Crouch", true, false));
        }
        
        protected override void OnEnter() {
            base.OnEnter();
            ctx.isCrouching = true;

            col = ctx.coll as CapsuleCollider2D;
            if (col != null) {
                // сохранение оригинала
                originalColliderSize = col.size;
                originalColliderOffset = col.offset;

                // уменьшаем высоту
                Vector2 newSize = new Vector2(originalColliderSize.x, originalColliderSize.y * ctx.stats.CrouchHeightMultiplier);
                // сдвигаем оффсет вниз так, чтобы низ коллайдера оставался на месте
                float deltaY = (originalColliderSize.y - newSize.y) * 0.5f;
                Vector2 newOffset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - deltaY);

                col.size = newSize;
                col.offset = newOffset;
            }
        }
        
        protected override void OnExit() {
            base.OnExit();
            ctx.isCrouching = false;

            if (col != null) {
                col.size = originalColliderSize;
                col.offset = originalColliderOffset;
            }
        }

        protected override State GetTransition() {
            if (ctx.ceilingAbove) return null;
            if (!ctx.input.CrouchHeld) return Machine != null ? Machine.GetState<Idle>() : null;
            return ctx.grounded ? null : (Machine != null ? Machine.GetState<Airborne>() : null);
        }

        protected override void OnUpdate(float deltaTime) {
            // ограничиваем горизонтальную скорость в приседе
            if (ctx.stats != null) {
                float target = 0f;
                if (Mathf.Abs(ctx.input.Move.x) > 0.01f) target = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.stats.CrouchSpeedMultiplier;
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, target, ctx.stats.Acceleration * deltaTime);
            }
            base.OnUpdate(deltaTime);
        }
    }
}

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
            ctx.currentSpeedMultiplier = ctx.stats.CrouchSpeedMultiplier;
            ctx.isCrouching = true;
            ctx.currentNoiseRadius = ctx.stats.CrouchNoiseRadius;
            ctx.currentFootstepInterval = ctx.stats.CrouchFootstepInterval;

            col = ctx.coll as CapsuleCollider2D;
            if (col != null) {
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
            
            base.OnEnter();
        }
        
        protected override void OnExit() {
            ctx.isCrouching = false;
            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;

            if (col != null) {
                col.size = originalColliderSize;
                col.offset = originalColliderOffset;
            }
            
            base.OnExit();
        }

        protected override State GetTransition() {
            if (ctx.isHiding) return Machine != null ? Machine.GetState<Hide>() : null;
            if (ctx.ceilingAbove) return null;
            if (!ctx.WantsCrouch) 
            {
                //if (Mathf.Abs(ctx.input.Move.x) > 0.01f) return Machine != null ? Machine.GetState<Move>() : null;
                if (ctx.HasMovementIntent) return Machine != null ? Machine.GetState<Move>() : null;
                return Machine != null ? Machine.GetState<Idle>() : null;
            }
            return null;
        }

    }
}

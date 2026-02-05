using UnityEngine;

namespace HSM {
    public class Crouch : GroundedStateBase {
        readonly PlayerContext2D ctx;
        readonly Collider2D[] headroomHits = new Collider2D[8];

        public Crouch(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) {
            this.ctx = ctx;
        }

        protected override void OnEnter() {
            SetCrouchCollider(true);
        }

        protected override void OnExit() {
            SetCrouchCollider(false);
        }

        protected override void OnUpdate(float deltaTime) {
            if (HasHeadroom()) {
                HandleJump(deltaTime);
            } else if (ctx.jumpToConsume) {
                // Prevent a stuck jump request while there's no headroom.
                ctx.jumpToConsume = false;
            }

            HandleDirection(deltaTime);
            HandleGravity(deltaTime);
            ApplyMovement();
        }

        protected override State GetTransition() {
            if (ctx.frameInput.CrouchHeld) return null;

            if (!HasHeadroom()) return null;

            if (Mathf.Abs(ctx.frameInput.Move.x) > 0.01f) {
                return ctx.frameInput.RunHeld ? ((Grounded)Parent).Run : ((Grounded)Parent).Walk;
            }

            return ((Grounded)Parent).Idle;
        }

        protected override float GetMaxSpeed() => Stats.MaxSpeed * Stats.CrouchSpeedMultiplier;

        bool HasHeadroom() {
            if (ctx.col == null || ctx.stats == null) return true;

            var col = ctx.col;
            var scale = col.transform.lossyScale;
            var size = new Vector2(
                ctx.colliderSize.x * Mathf.Abs(scale.x),
                ctx.colliderSize.y * Mathf.Abs(scale.y)
            );
            // Slightly shrink to avoid getting stuck due to tiny overlaps.
            const float skin = 0.02f;
            size = new Vector2(
                Mathf.Max(0.01f, size.x - skin),
                Mathf.Max(0.01f, size.y - skin)
            );

            var center = col.transform.TransformPoint(ctx.colliderOffset);
            var angle = col.transform.eulerAngles.z;

            var filter = new ContactFilter2D {
                useLayerMask = true,
                layerMask = ~ctx.stats.PlayerLayer,
                useTriggers = false
            };

            var hitCount = Physics2D.OverlapCapsule(
                center,
                size,
                col.direction,
                angle,
                filter,
                headroomHits
            );

            if (hitCount == 0) return true;

            // Ignore any collider belonging to the player.
            for (int i = 0; i < hitCount; i++) {
                var hit = headroomHits[i];
                if (hit == null) continue;
                if (hit == col) continue;
                if (ctx.rb != null && hit.attachedRigidbody == ctx.rb) continue;
                return false;
            }

            // If we filled the buffer with only self-colliders, do a definitive check.
            if (hitCount >= headroomHits.Length) {
                var allHits = Physics2D.OverlapCapsuleAll(
                    center,
                    size,
                    col.direction,
                    angle,
                    ~ctx.stats.PlayerLayer
                );
                for (int i = 0; i < allHits.Length; i++) {
                    var hit = allHits[i];
                    if (hit == null) continue;
                    if (hit == col) continue;
                    if (ctx.rb != null && hit.attachedRigidbody == ctx.rb) continue;
                    return false;
                }
            }

            return true;
        }
    }
}

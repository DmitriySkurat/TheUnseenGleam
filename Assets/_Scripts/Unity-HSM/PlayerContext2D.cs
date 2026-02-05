using UnityEngine;

namespace HSM {
    [System.Serializable]
    public class PlayerContext2D {
        public ScriptableStats stats;
        public Rigidbody2D rb;
        public CapsuleCollider2D col;
        public PlayerCollisionProbe2D collisionProbe;
        public Vector2 colliderSize;
        public Vector2 colliderOffset;

        public PlayerFrameInput frameInput;

        public bool jumpToConsume;
        public bool bufferedJumpUsable;
        public bool endedJumpEarly;
        public bool coyoteUsable;
        public float timeJumpWasPressed;

        public bool IsGrounded => collisionProbe != null && collisionProbe.IsGrounded;

        public bool HasBufferedJump =>
            bufferedJumpUsable && Time.fixedTime < timeJumpWasPressed + stats.JumpBuffer;

        public bool CanUseCoyote =>
            coyoteUsable && !IsGrounded && Time.fixedTime < collisionProbe.FrameLeftGrounded + stats.CoyoteTime;

        public void SetCrouchCollider(bool isCrouching) {
            if (col == null) return;
            if (!isCrouching) {
                col.size = colliderSize;
                col.offset = colliderOffset;
                return;
            }

            float heightPercent = Mathf.Clamp(stats.CrouchHeightPercent, 0.1f, 1f);
            var newSize = new Vector2(colliderSize.x, colliderSize.y * heightPercent);
            var heightDelta = newSize.y - colliderSize.y;
            var newOffset = new Vector2(colliderOffset.x, colliderOffset.y + heightDelta * 0.5f);

            col.size = newSize;
            col.offset = newOffset;
        }
    }
}

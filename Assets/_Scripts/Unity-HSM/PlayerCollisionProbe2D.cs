using System;
using UnityEngine;

namespace HSM {
    public readonly struct CollisionProbeResult2D {
        public readonly bool GroundHit;
        public readonly bool CeilingHit;

        public CollisionProbeResult2D(bool groundHit, bool ceilingHit) {
            GroundHit = groundHit;
            CeilingHit = ceilingHit;
        }
    }

    public sealed class PlayerCollisionProbe2D : MonoBehaviour {
        bool cachedQueryStartInColliders;

        public bool IsGrounded { get; private set; }
        public float FrameLeftGrounded { get; private set; } = float.MinValue;
        public event Action<bool, float> GroundedChanged;

        void Awake() {
            cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        public CollisionProbeResult2D Probe(ScriptableStats stats, CapsuleCollider2D collider, float velocityY) {
            Physics2D.queriesStartInColliders = false;

            bool groundHit = Physics2D.CapsuleCast(
                collider.bounds.center,
                collider.size,
                collider.direction,
                0,
                Vector2.down,
                stats.GrounderDistance,
                ~stats.PlayerLayer);

            bool ceilingHit = Physics2D.CapsuleCast(
                collider.bounds.center,
                collider.size,
                collider.direction,
                0,
                Vector2.up,
                stats.GrounderDistance,
                ~stats.PlayerLayer);

            if (!IsGrounded && groundHit) {
                IsGrounded = true;
                GroundedChanged?.Invoke(true, Mathf.Abs(velocityY));
            } else if (IsGrounded && !groundHit) {
                IsGrounded = false;
                FrameLeftGrounded = Time.fixedTime;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = cachedQueryStartInColliders;

            return new CollisionProbeResult2D(groundHit, ceilingHit);
        }
    }
}

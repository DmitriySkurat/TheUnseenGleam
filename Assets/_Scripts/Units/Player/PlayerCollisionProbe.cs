using System;
using UnityEngine;

public readonly struct CollisionProbeResult
{
    public readonly bool GroundHit;
    public readonly bool CeilingHit;

    public CollisionProbeResult(bool groundHit, bool ceilingHit)
    {
        GroundHit = groundHit;
        CeilingHit = ceilingHit;
    }
}

public sealed class PlayerCollisionProbe : MonoBehaviour
{
    private bool _cachedQueryStartInColliders;

    public bool IsGrounded { get; private set; }
    public float FrameLeftGrounded { get; private set; } = float.MinValue;
    public event Action<bool, float> GroundedChanged;

    private void Awake()
    {
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    public CollisionProbeResult Probe(ScriptableStats stats, CapsuleCollider2D collider, float frameVelocityY)
    {
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

        if (!IsGrounded && groundHit)
        {
            IsGrounded = true;
            GroundedChanged?.Invoke(true, Mathf.Abs(frameVelocityY));
        }
        else if (IsGrounded && !groundHit)
        {
            IsGrounded = false;
            FrameLeftGrounded = Time.fixedTime;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;

        return new CollisionProbeResult(groundHit, ceilingHit);
    }
}

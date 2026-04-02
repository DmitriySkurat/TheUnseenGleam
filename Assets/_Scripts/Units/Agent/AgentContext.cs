using System;
using UnityEngine;
using PlatNav;

[Serializable]
public class AgentContext
{
    // ===== STATS =====
    public AgentScriptableStats stats;

    // ===== COMPONENTS =====
    public Transform transform;
    public Rigidbody2D rb;
    public PlatNavHandler nav;

    // ===== SPAWN =====
    public Vector2 spawnPosition;

    // ===== PATROL =====
    // Inspector-assigned transforms (take priority when set)
    public Transform[] patrolPointTransforms;
    // Auto-generated fallback positions
    public Vector2[] patrolPositions;

    public int currentPatrolIndex;
    public float patrolWaitTimer;

    // ===== DERIVED =====
    public bool IsWaitingAtPoint => patrolWaitTimer > 0f;

    public int PatrolCount => patrolPointTransforms != null && patrolPointTransforms.Length > 0
        ? patrolPointTransforms.Length
        : (patrolPositions != null ? patrolPositions.Length : 0);

    public Vector2 CurrentPatrolPosition => patrolPointTransforms != null && patrolPointTransforms.Length > 0
        ? (Vector2)patrolPointTransforms[currentPatrolIndex].position
        : patrolPositions[currentPatrolIndex];
}

using System;
using UnityEngine;
using PlatNav;

public enum SuspicionSource { None, Vision, Noise }

[Serializable]
public class AgentContext
{
    // ===== STATS =====
    public AgentScriptableStats stats;

    // ===== COMPONENTS =====
    public Transform transform;
    public Rigidbody2D rb;
    public PlatNavHandler nav;
    public AgentVision vision;
    public AgentHearing hearing;

    // ===== SPAWN =====
    public Vector2 spawnPosition;

    // ===== PATROL =====
    // Inspector-assigned transforms (take priority when set)
    public Transform[] patrolPointTransforms;
    // Auto-generated fallback positions
    public Vector2[] patrolPositions;

    public int currentPatrolIndex;
    public float patrolWaitTimer;

    // ===== SUSPICION =====
    public SuspicionSource suspicionSource;
    public float suspicionTimer;
    // Loudness of the loudest noise heard so far during this suspicion window
    public float suspicionNoiseLoudness;
    // How many noise events were heard during the current suspicion window
    public int noisesHeardDuringSuspicion;
    // World position that triggered suspicion (last seen / heard)
    public Vector2 suspicionPosition;

    // ===== SEARCH =====
    // Countdown while standing at a wander waypoint
    public float searchWaitTimer;
    // Countdown for the total wander phase duration
    public float searchWanderTimer;

    // ===== CHASE =====
    public Transform playerTransform;
    // Time elapsed since the agent last had direct sight of the player during chase
    public float chaseVisionLostTimer;

    // ===== PENDING NOISE (written by AgentStateDriver, read by states) =====
    public bool pendingNoiseAlert;
    public float pendingNoiseLoudness;
    public Vector2 pendingNoisePosition;

    // ===== DERIVED =====
    public bool IsWaitingAtPoint => patrolWaitTimer > 0f;

    public int PatrolCount => patrolPointTransforms != null && patrolPointTransforms.Length > 0
        ? patrolPointTransforms.Length
        : (patrolPositions != null ? patrolPositions.Length : 0);

    public Vector2 CurrentPatrolPosition => patrolPointTransforms != null && patrolPointTransforms.Length > 0
        ? (Vector2)patrolPointTransforms[currentPatrolIndex].position
        : patrolPositions[currentPatrolIndex];
}

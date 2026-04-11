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
    public AgentInteractor interactor;
    public AgentLightSensor lightSensor;

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
    public Rigidbody2D playerRb;
    public PlayerHealth playerHealth;
    // Time elapsed since the agent last had direct sight of the player during chase
    public float chaseVisionLostTimer;

    // ===== PREDICTION CHASE =====
    // Player's velocity at the last moment they were seen (used to predict where they went)
    public Vector2 predictionPlayerVelocity;
    // Countdown for the total time budget of PredictionChase
    public float predictionTimer;

    // ===== GRAB =====
    public PlayerContext playerCtx;
    public float attackFirstHitTimer;
    public float grabCooldownTimer;
    // true после первого захвата в текущей погоне; сбрасывается при входе в AgentChase
    public bool grabOccurredInChase;

    // ===== BLINDING =====
    // true если агент в данный момент ослеплён светом игрока
    public bool isBlindedByPlayer;
    // true если агент ослеплён статическим (не игроцким) источником света
    public bool isBlindedByEnvironment;
    // Мировая позиция источника, который сейчас слепит агента
    public Vector2 blindingSourcePosition;
    // Сколько секунд агент непрерывно ослеплён светом игрока (растёт во всех состояниях)
    public float blindedByPlayerTimer;

    // ===== PENDING NOISE (written by AgentStateDriver, read by states) =====
    public bool pendingNoiseAlert;
    public float pendingNoiseRadius;
    public Vector2 pendingNoisePosition;

    // ===== ALERT (agent-to-agent cooperation) =====
    public AgentAlertSystem alertSystem;
    public bool alertPending;
    public Vector2 alertPosition;

    // ===== DERIVED =====
    public bool IsTraversingLink => nav != null && nav.State == PlatNavState.TraversingLink;
    public bool IsWaitingAtPoint => patrolWaitTimer > 0f;

    public int PatrolCount => patrolPointTransforms != null && patrolPointTransforms.Length > 0
        ? patrolPointTransforms.Length
        : (patrolPositions != null ? patrolPositions.Length : 0);

    public Vector2 CurrentPatrolPosition => patrolPointTransforms != null && patrolPointTransforms.Length > 0
        ? (Vector2)patrolPointTransforms[currentPatrolIndex].position
        : patrolPositions[currentPatrolIndex];
}

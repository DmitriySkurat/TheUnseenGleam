using PlatNav;
using UnityEngine;

public class EnemyContext
{
    // ===== CONFIG =====
    public Transform[] patrolPoints;
    public float patrolHalfWidth;
    public float patrolSpeed;
    public float patrolWaitTime;
    public float chaseSpeed;
    public float retargetDistance;
    public float investigateSpeed;
    public float investigateWaitTime;
    public float searchSpeed;
    public float searchHalfWidth;
    public float searchDuration;
    public float searchWaitTime;
    public float returnSpeed;
    public float lightEscapeDistance;
    public float lightEscapeSpeed;
    public float contactDamage;
    public float contactDamageCooldown;
    public float contactDamageRangeFallback;
    public float contactAttackWindup;

    // ===== REFERENCES =====
    public PlatNavHandler nav;
    public AgentVision vision;
    public AgentHearing hearing;
    public AgentLightSensor lightSensor;
    public Renderer renderer;
    public Transform selfTransform;
    public Transform playerTransform;
    public Collider2D selfCollider;
    public Rigidbody2D rb;
    public PlayerContext playerContext;

    // ===== PATROL STATE =====
    public Vector2 patrolOrigin;
    public Vector2[] patrolRoute;
    public int patrolIndex;
    public int patrolDirection = 1;

    // ===== CHASE STATE =====
    public Vector2 lastKnownPlayerPosition;
    public bool hasKnownPlayerPosition;
    public bool hasDetectedPlayer;
    public bool usingVisualChase;

    // ===== INVESTIGATE STATE =====
    public Vector2 investigationTarget;

    // ===== SEARCH STATE =====
    public Vector2 searchCenter;
    public readonly Vector2[] searchRoute = new Vector2[2];
    public int searchIndex;
    public int searchDirection = 1;
    public float searchTimer;

    // ===== RETURN STATE =====
    public Vector2 returnTarget;

    // ===== SHARED RUNTIME STATE =====
    public bool isWaiting;
    public float waitTimer;
    public Vector2 manualDestination;
    public bool manualCommandActive;
    public bool manualCommandFailed;

    // ===== BLIND RUN STATE =====
    public bool blindRunLocked;
    public Vector2 blindRunDirection;

    // ===== CONTACT DAMAGE STATE =====
    public float nextContactDamageTime;
    public float contactAttackTimer;

    // ===== DERIVED =====
    public bool IsTraversingLink => nav != null && nav.State == PlatNavState.TraversingLink;
    public bool CanSeePlayer => vision != null && vision.CanSeePlayer && playerTransform != null;
}

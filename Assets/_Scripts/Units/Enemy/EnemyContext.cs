using System.Collections.Generic;
using PlatNav;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    public Transform selfTransform;
    public Transform playerTransform;
    public Collider2D selfCollider;
    public Rigidbody2D rb;

    // ===== PLAYER CONTEXT (for contact damage) =====
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

    // ===== HELPERS =====

    public bool TryMoveTo(Vector2 targetPosition, float speed)
    {
        if (nav == null)
            return false;

        float minRetarget = Mathf.Max(0.01f, retargetDistance);
        bool needsNew = !manualCommandActive
            || manualCommandFailed
            || (manualDestination - targetPosition).sqrMagnitude > minRetarget * minRetarget;

        if (!needsNew)
            return !manualCommandFailed;

        ForceMoveTo(targetPosition, speed);
        return !manualCommandFailed;
    }

    public void ForceMoveTo(Vector2 targetPosition, float speed)
    {
        if (nav == null)
            return;

        nav.SetBehaviour(PlatNavBehaviour.FollowTarget);
        nav.SetTarget(null);
        nav.Abort();
        usingVisualChase = false;

        manualDestination = targetPosition;
        manualCommandFailed = !nav.MoveTo(targetPosition, speed);
        manualCommandActive = !manualCommandFailed;
    }

    public bool HasCompletedManualMove()
    {
        if (!manualCommandActive || nav == null)
            return false;

        if (nav.HasPath || nav.State != PlatNavState.Idle)
            return false;

        ResetManualCommand();
        return true;
    }

    public void BeginWait(float duration)
    {
        isWaiting = duration > 0f;
        waitTimer = duration;
        ResetManualCommand();
    }

    public bool UpdateWaitTimer()
    {
        if (!isWaiting)
            return false;

        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f)
            return false;

        isWaiting = false;
        waitTimer = 0f;
        return true;
    }

    public void ResetWait()
    {
        isWaiting = false;
        waitTimer = 0f;
    }

    public void ResetManualCommand()
    {
        manualCommandActive = false;
        manualCommandFailed = false;
    }

    public void BeginVisualChase()
    {
        if (nav == null || playerTransform == null)
            return;

        if (usingVisualChase)
            return;

        nav.Abort();
        nav.SetBehaviour(PlatNavBehaviour.FollowTarget);
        nav.SetTarget(playerTransform);
        nav.MoveTo(playerTransform.position, chaseSpeed);
        usingVisualChase = true;
        ResetManualCommand();
    }

    public void StopVisualChase()
    {
        if (nav == null)
            return;

        nav.SetTarget(null);
        if (usingVisualChase)
            nav.Abort();

        usingVisualChase = false;
    }

    public void AdvancePatrolIndex()
    {
        if (patrolRoute == null || patrolRoute.Length <= 1)
        {
            patrolIndex = 0;
            return;
        }

        patrolIndex = GetNextBounceIndex(patrolIndex, ref patrolDirection, patrolRoute.Length);
    }

    public void AdvanceSearchIndex()
    {
        searchIndex = GetNextBounceIndex(searchIndex, ref searchDirection, searchRoute.Length);
    }

    public void BuildPatrolRoute()
    {
        patrolOrigin = selfTransform.position;

        var points = new List<Vector2>();
        if (patrolPoints != null)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                    points.Add(patrolPoints[i].position);
            }
        }

        if (points.Count >= 2)
        {
            patrolRoute = points.ToArray();
            return;
        }

        Vector2 offset = Vector2.right * patrolHalfWidth;
        patrolRoute = new[] { patrolOrigin - offset, patrolOrigin + offset };
    }

    public int GetInitialPatrolIndex()
    {
        if (patrolRoute == null || patrolRoute.Length == 0)
            return 0;

        return GetClosestIndex(patrolRoute, selfTransform.position);
    }

    public Vector2 GetPatrolReturnPoint()
    {
        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            return patrolPoints[0].position;

        return patrolOrigin;
    }

    public bool IsTraversingLink()
    {
        return nav != null && nav.State == PlatNavState.TraversingLink;
    }

    public void UpdateKnownPlayerPosition(Vector2 position)
    {
        if (lightSensor != null && lightSensor.IsBlindedAt(position, out _, out _))
            return;

        lastKnownPlayerPosition = position;
        hasKnownPlayerPosition = true;
        hasDetectedPlayer = true;
    }

    public bool TryHandlePatrolLightResponse()
    {
        if (lightSensor == null)
            return false;

        if (!lightSensor.IsBlinded(out Light2D strongestLight, out _))
            return false;

        bool isMirrorLight = strongestLight != null && strongestLight.GetComponent<MirrorLightSource>() != null;
        bool isStandingStill = isWaiting || (nav != null && nav.State == PlatNavState.Idle && !nav.HasPath);

        if (isMirrorLight && isStandingStill)
        {
            ResetManualCommand();
            return true;
        }

        if (isMirrorLight)
            return false;

        ResetWait();
        Vector2 escapeTarget = (Vector2)selfTransform.position + GetFacingDirection() * lightEscapeDistance;
        TryMoveTo(escapeTarget, lightEscapeSpeed);
        return true;
    }

    public bool TryHandleBlindRun()
    {
        if (lightSensor == null)
        {
            blindRunLocked = false;
            return false;
        }

        if (!lightSensor.IsBlinded(out _, out _))
        {
            blindRunLocked = false;
            return false;
        }

        if (!blindRunLocked)
        {
            blindRunDirection = GetFacingDirection();
            blindRunLocked = true;
            ResetWait();
            StopVisualChase();
            ResetManualCommand();
            if (nav != null)
                nav.Abort();
            hasKnownPlayerPosition = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(blindRunDirection.x * lightEscapeSpeed, rb.linearVelocity.y);
            Vector3 scale = selfTransform.localScale;
            scale.x = blindRunDirection.x >= 0f ? 1f : -1f;
            selfTransform.localScale = scale;
        }

        return true;
    }

    public void TryApplyContactDamage()
    {
        if (contactDamage <= 0f)
            return;

        if (lightSensor != null && lightSensor.IsBlinded(out _, out _))
        {
            contactAttackTimer = 0f;
            return;
        }

        if (Time.time < nextContactDamageTime)
        {
            contactAttackTimer = 0f;
            return;
        }

        if (playerContext == null || playerContext.health == null || !playerContext.isAlive || playerTransform == null)
        {
            contactAttackTimer = 0f;
            return;
        }

        if (!IsPlayerInContactRange())
        {
            contactAttackTimer = 0f;
            return;
        }

        if (contactAttackWindup > 0f)
        {
            contactAttackTimer += Time.deltaTime;
            if (contactAttackTimer < contactAttackWindup)
                return;
        }

        playerContext.health.TakeDamage(contactDamage);
        nextContactDamageTime = Time.time + contactDamageCooldown;
        contactAttackTimer = 0f;
    }

    public void ResolvePlayerTransform()
    {
        if (playerTransform != null)
            return;

        if (playerContext == null)
            playerContext = Services.Get<PlayerContext>();

        if (playerContext != null)
            playerTransform = playerContext.transform;
    }

    public Vector2 GetFacingDirection()
    {
        return selfTransform.localScale.x >= 0f ? Vector2.right : Vector2.left;
    }

    public Vector2 AdjustPositionAwayFromLight(Vector2 position)
    {
        if (lightSensor == null || !lightSensor.IsBlindedAt(position, out _, out _))
            return position;

        Vector2 dir = (position - (Vector2)selfTransform.position).normalized;
        if (dir.sqrMagnitude < 0.01f)
            dir = Vector2.right;

        Vector2 adjusted = position + dir * lightEscapeDistance;

        if (lightSensor.IsBlindedAt(adjusted, out _, out _))
        {
            Vector2 perp = new Vector2(-dir.y, dir.x);
            adjusted = position + perp * lightEscapeDistance;
        }

        return adjusted;
    }

    private bool IsPlayerInContactRange()
    {
        if (selfCollider != null && playerContext != null && playerContext.coll != null)
            return selfCollider.Distance(playerContext.coll).isOverlapped;

        float sqrDist = ((Vector2)selfTransform.position - (Vector2)playerTransform.position).sqrMagnitude;
        return sqrDist <= contactDamageRangeFallback * contactDamageRangeFallback;
    }

    public static int GetClosestIndex(IReadOnlyList<Vector2> points, Vector2 worldPosition)
    {
        int best = 0;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            float sqr = (points[i] - worldPosition).sqrMagnitude;
            if (sqr >= bestSqr)
                continue;

            bestSqr = sqr;
            best = i;
        }

        return best;
    }

    public static int GetNextBounceIndex(int current, ref int direction, int length)
    {
        if (length <= 1)
            return 0;

        int next = current + direction;
        if (next >= length || next < 0)
        {
            direction *= -1;
            next = current + direction;
        }

        return Mathf.Clamp(next, 0, length - 1);
    }
}

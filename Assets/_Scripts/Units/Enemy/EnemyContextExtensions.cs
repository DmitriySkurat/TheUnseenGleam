using System.Collections.Generic;
using PlatNav;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class EnemyContextExtensions
{
    public static bool TryMoveTo(this EnemyContext ctx, Vector2 targetPosition, float speed)
    {
        if (ctx.nav == null)
            return false;

        float minRetarget = Mathf.Max(0.01f, ctx.retargetDistance);
        bool needsNew = !ctx.manualCommandActive
            || ctx.manualCommandFailed
            || (ctx.manualDestination - targetPosition).sqrMagnitude > minRetarget * minRetarget;

        if (!needsNew)
            return !ctx.manualCommandFailed;

        ctx.ForceMoveTo(targetPosition, speed);
        return !ctx.manualCommandFailed;
    }

    public static void ForceMoveTo(this EnemyContext ctx, Vector2 targetPosition, float speed)
    {
        if (ctx.nav == null)
            return;

        ctx.nav.SetBehaviour(PlatNavBehaviour.FollowTarget);
        ctx.nav.SetTarget(null);
        ctx.nav.Abort();
        ctx.usingVisualChase = false;

        ctx.manualDestination = targetPosition;
        ctx.manualCommandFailed = !ctx.nav.MoveTo(targetPosition, speed);
        ctx.manualCommandActive = !ctx.manualCommandFailed;
    }

    public static bool HasCompletedManualMove(this EnemyContext ctx)
    {
        if (!ctx.manualCommandActive || ctx.nav == null)
            return false;

        if (ctx.nav.HasPath || ctx.nav.State != PlatNavState.Idle)
            return false;

        ctx.ResetManualCommand();
        return true;
    }

    public static void BeginWait(this EnemyContext ctx, float duration)
    {
        ctx.isWaiting = duration > 0f;
        ctx.waitTimer = duration;
        ctx.ResetManualCommand();
    }

    public static bool UpdateWaitTimer(this EnemyContext ctx)
    {
        if (!ctx.isWaiting)
            return false;

        ctx.waitTimer -= Time.deltaTime;
        if (ctx.waitTimer > 0f)
            return false;

        ctx.isWaiting = false;
        ctx.waitTimer = 0f;
        return true;
    }

    public static void ResetWait(this EnemyContext ctx)
    {
        ctx.isWaiting = false;
        ctx.waitTimer = 0f;
    }

    public static void ResetManualCommand(this EnemyContext ctx)
    {
        ctx.manualCommandActive = false;
        ctx.manualCommandFailed = false;
    }

    public static void BeginVisualChase(this EnemyContext ctx)
    {
        if (ctx.nav == null || ctx.playerTransform == null)
            return;

        if (ctx.usingVisualChase)
            return;

        ctx.nav.Abort();
        ctx.nav.SetBehaviour(PlatNavBehaviour.FollowTarget);
        ctx.nav.SetTarget(ctx.playerTransform);
        ctx.nav.MoveTo(ctx.playerTransform.position, ctx.chaseSpeed);
        ctx.usingVisualChase = true;
        ctx.ResetManualCommand();
    }

    public static void StopVisualChase(this EnemyContext ctx)
    {
        if (ctx.nav == null)
            return;

        ctx.nav.SetTarget(null);
        if (ctx.usingVisualChase)
            ctx.nav.Abort();

        ctx.usingVisualChase = false;
    }

    public static void AdvancePatrolIndex(this EnemyContext ctx)
    {
        if (ctx.patrolRoute == null || ctx.patrolRoute.Length <= 1)
        {
            ctx.patrolIndex = 0;
            return;
        }

        ctx.patrolIndex = GetNextBounceIndex(ctx.patrolIndex, ref ctx.patrolDirection, ctx.patrolRoute.Length);
    }

    public static void AdvanceSearchIndex(this EnemyContext ctx)
    {
        ctx.searchIndex = GetNextBounceIndex(ctx.searchIndex, ref ctx.searchDirection, ctx.searchRoute.Length);
    }

    public static void BuildPatrolRoute(this EnemyContext ctx)
    {
        ctx.patrolOrigin = ctx.selfTransform.position;

        var points = new List<Vector2>();
        if (ctx.patrolPoints != null)
        {
            for (int i = 0; i < ctx.patrolPoints.Length; i++)
            {
                if (ctx.patrolPoints[i] != null)
                    points.Add(ctx.patrolPoints[i].position);
            }
        }

        if (points.Count >= 2)
        {
            ctx.patrolRoute = points.ToArray();
            return;
        }

        Vector2 offset = Vector2.right * ctx.patrolHalfWidth;
        ctx.patrolRoute = new[] { ctx.patrolOrigin - offset, ctx.patrolOrigin + offset };
    }

    public static int GetInitialPatrolIndex(this EnemyContext ctx)
    {
        if (ctx.patrolRoute == null || ctx.patrolRoute.Length == 0)
            return 0;

        return GetClosestIndex(ctx.patrolRoute, ctx.selfTransform.position);
    }

    public static Vector2 GetPatrolReturnPoint(this EnemyContext ctx)
    {
        if (ctx.patrolPoints != null && ctx.patrolPoints.Length > 0 && ctx.patrolPoints[0] != null)
            return ctx.patrolPoints[0].position;

        return ctx.patrolOrigin;
    }

    public static void UpdateKnownPlayerPosition(this EnemyContext ctx, Vector2 position)
    {
        if (ctx.lightSensor != null && ctx.lightSensor.IsBlindedAt(position, out _, out _))
            return;

        ctx.lastKnownPlayerPosition = position;
        ctx.hasKnownPlayerPosition = true;
        ctx.hasDetectedPlayer = true;
    }

    public static bool TryHandlePatrolLightResponse(this EnemyContext ctx)
    {
        if (ctx.lightSensor == null)
            return false;

        if (!ctx.lightSensor.IsBlinded(out Light2D strongestLight, out _))
            return false;

        bool isMirrorLight = strongestLight != null && strongestLight.GetComponent<MirrorLightSource>() != null;
        bool isStandingStill = ctx.isWaiting || (ctx.nav != null && ctx.nav.State == PlatNavState.Idle && !ctx.nav.HasPath);

        if (isMirrorLight && isStandingStill)
        {
            ctx.ResetManualCommand();
            return true;
        }

        if (isMirrorLight)
            return false;

        ctx.ResetWait();
        Vector2 escapeTarget = (Vector2)ctx.selfTransform.position + ctx.GetFacingDirection() * ctx.lightOverrunDistance;
        ctx.TryMoveTo(escapeTarget, ctx.lightEscapeSpeed);
        return true;
    }

    public static void TryApplyContactDamage(this EnemyContext ctx)
    {
        if (ctx.contactDamage <= 0f)
            return;

        if (ctx.lightSensor != null && ctx.lightSensor.IsBlinded(out _, out _))
        {
            ctx.contactAttackTimer = 0f;
            return;
        }

        if (Time.time < ctx.nextContactDamageTime)
        {
            ctx.contactAttackTimer = 0f;
            return;
        }

        if (ctx.playerContext == null || ctx.playerContext.health == null || !ctx.playerContext.isAlive || ctx.playerTransform == null)
        {
            ctx.contactAttackTimer = 0f;
            return;
        }

        if (!ctx.IsPlayerInContactRange())
        {
            ctx.contactAttackTimer = 0f;
            return;
        }

        if (ctx.contactAttackWindup > 0f)
        {
            ctx.contactAttackTimer += Time.deltaTime;
            if (ctx.contactAttackTimer < ctx.contactAttackWindup)
                return;
        }

        ctx.playerContext.health.TakeDamage(ctx.contactDamage);
        ctx.nextContactDamageTime = Time.time + ctx.contactDamageCooldown;
        ctx.contactAttackTimer = 0f;
    }

    public static void ResolvePlayerTransform(this EnemyContext ctx)
    {
        if (ctx.playerTransform != null)
            return;

        if (ctx.playerContext == null)
            ctx.playerContext = Services.Get<PlayerContext>();

        if (ctx.playerContext != null)
            ctx.playerTransform = ctx.playerContext.transform;
    }

    public static Vector2 GetFacingDirection(this EnemyContext ctx)
        => ctx.selfTransform.localScale.x >= 0f ? Vector2.right : Vector2.left;

    public static Vector2 AdjustPositionAwayFromLight(this EnemyContext ctx, Vector2 position)
    {
        if (ctx.lightSensor == null || !ctx.lightSensor.IsBlindedAt(position, out _, out _))
            return position;

        Vector2 dir = (position - (Vector2)ctx.selfTransform.position).normalized;
        if (dir.sqrMagnitude < 0.01f)
            dir = Vector2.right;

        Vector2 adjusted = position + dir * ctx.lightOverrunDistance;

        if (ctx.lightSensor.IsBlindedAt(adjusted, out _, out _))
        {
            Vector2 perp = new Vector2(-dir.y, dir.x);
            adjusted = position + perp * ctx.lightOverrunDistance;
        }

        return adjusted;
    }

    private static bool IsPlayerInContactRange(this EnemyContext ctx)
    {
        if (ctx.selfCollider != null && ctx.playerContext != null && ctx.playerContext.coll != null)
            return ctx.selfCollider.Distance(ctx.playerContext.coll).isOverlapped;

        float sqrDist = ((Vector2)ctx.selfTransform.position - (Vector2)ctx.playerTransform.position).sqrMagnitude;
        return sqrDist <= ctx.contactDamageRangeFallback * ctx.contactDamageRangeFallback;
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

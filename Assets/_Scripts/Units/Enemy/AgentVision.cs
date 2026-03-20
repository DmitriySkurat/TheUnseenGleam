using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AgentVision : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Vision Settings")]
    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 90f;

    [SerializeField, Min(0f)] private float viewDistance = 5f;
    [Range(0f, 360f)]
    [SerializeField] private float maxViewAngle = 90f;
    [SerializeField, Min(0f)] private float maxViewDistance = 5f;
    [SerializeField, Min(0f)] private float visibilityMultiplier = 1f;
    [SerializeField] private LayerMask occlusionMask;

    [Header("Light Glare")]
    [SerializeField] private bool reduceVisionFromLight = true;
    [SerializeField, Min(0.01f)] private float glareSampleStep = 0.25f;
    [SerializeField, Min(1)] private int glareMaxSamples = 24;
    [SerializeField, Min(0f)] private float maxGlareStrength = 2f;
    [SerializeField, Range(0f, 1f)] private float minGlareVisibilityMultiplier = 0.3f;
    [SerializeField, Range(0f, 45f)] private float glareRayAngleOffset = 5f;
    [SerializeField] private bool useLightOcclusion = true;
    [SerializeField] private bool blindWhenInStrongLight = true;
    [SerializeField, Min(0f)] private float blindStartStrength = 1f;
    [SerializeField, Min(0f)] private float blindFullStrength = 2f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private Color maxGizmoColor = new Color(0.2f, 0.8f, 1f, 0.6f);
    [SerializeField] private Color lastSeenColor = new Color(1f, 0.4f, 0.1f, 0.9f);
    [SerializeField, Min(0f)] private float lastSeenMarkerRadius = 0.2f;

    [Header("Debug Glare Rays")]
    [SerializeField] private bool drawGlareRays = true;
    [SerializeField] private Color glareRayMinColor = new Color(0.2f, 1f, 0.4f, 0.7f);
    [SerializeField] private Color glareRayMaxColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    [SerializeField, Min(0f)] private float glareSampleGizmoRadius = 0.05f;

    private PlayerContext _playerContext;
    private Transform _playerTransform;
    private Vector2? _lastSeenPosition;
    private bool _canSeePlayer;
    private LightSystem _lightSystem;
    private readonly Vector2[] _glareDirections = new Vector2[3];

    private struct LightSample
    {
        public Vector2 ToTarget;
        public float Distance;
        public float OuterRadius;
        public float OuterAngle;
        public float AngleToTarget;
    }

    public bool CanSeePlayer => _canSeePlayer;
    public bool HasLastSeenPosition => _lastSeenPosition.HasValue;
    public Vector2 LastSeenPosition => _lastSeenPosition ?? Vector2.zero;
    public float ViewAngle => viewAngle;
    public float ViewDistance => viewDistance;
    public float VisibilityMultiplier
    {
        get => visibilityMultiplier;
        set => visibilityMultiplier = Mathf.Max(0f, value);
    }
    
    public void Initialize()
    {
        _playerContext = Services.Get<PlayerContext>();
        _playerTransform = _playerContext.transform;
        _lightSystem = Services.Get<LightSystem>();
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            if (_playerContext == null)
            {
                _canSeePlayer = false;
                return;
            }

            _playerTransform = _playerContext.transform;
            if (_playerTransform == null)
            {
                _canSeePlayer = false;
                return;
            }
        }

        _canSeePlayer = CheckPlayerVisibility(_playerTransform.position);
        if (_canSeePlayer)
            _lastSeenPosition = _playerTransform.position;
    }

    private bool CheckPlayerVisibility(Vector2 targetPosition)
    {
        float baseViewDistance = GetEffectiveViewDistance();
        float effectiveViewAngle = GetEffectiveViewAngle();
        if (baseViewDistance <= 0f || effectiveViewAngle <= 0f)
            return false;

        Vector2 origin = transform.position;
        Vector2 toTarget = targetPosition - origin;
        if (toTarget.sqrMagnitude > baseViewDistance * baseViewDistance)
            return false;

        Vector2 forward = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(forward, toTarget);
        if (angle > effectiveViewAngle * 0.5f)
            return false;

        float effectiveViewDistance = GetEffectiveViewDistanceWithGlare();
        if (toTarget.sqrMagnitude > effectiveViewDistance * effectiveViewDistance)
            return false;

        if (occlusionMask.value != 0)
        {
            var hit = Physics2D.Linecast(origin, targetPosition, occlusionMask);
            if (hit.collider != null)
                return false;
        }

        return true;
    }

    private float GetEffectiveViewDistance()
    {
        float effective = viewDistance * Mathf.Max(0f, visibilityMultiplier);
        if (maxViewDistance > 0f)
            effective = Mathf.Min(effective, maxViewDistance);
        return effective;
    }

    private float GetEffectiveViewDistanceWithGlare()
    {
        float effective = GetEffectiveViewDistance();
        if (!reduceVisionFromLight || _lightSystem == null || effective <= 0f)
            return effective;

        Vector2 origin = transform.position;
        Vector2 forward = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        float rayDistance = GetGlareRayDistance(effective);
        FillGlareDirections(forward, _glareDirections);
        float glareStrength = EvaluateGlareStrength(origin, _glareDirections, rayDistance);
        float glareMultiplier = GetGlareVisibilityMultiplier(glareStrength);

        float blindMultiplier = GetBlindVisibilityMultiplier(origin);

        return effective * glareMultiplier * blindMultiplier;
    }

    private float GetEffectiveViewAngle()
    {
        float effective = viewAngle;
        if (maxViewAngle > 0f)
            effective = Mathf.Min(effective, maxViewAngle);
        return effective;
    }

    private float GetGlareVisibilityMultiplier(float glareStrength)
    {
        if (maxGlareStrength <= 0f)
            return 1f;

        float clampedMin = Mathf.Clamp01(minGlareVisibilityMultiplier);
        float t = Mathf.Clamp01(glareStrength / maxGlareStrength);
        return Mathf.Lerp(1f, clampedMin, t);
    }

    private float EvaluateGlareStrength(Vector2 origin, IReadOnlyList<Vector2> directions, float distance)
    {
        if (_lightSystem == null)
            return 0f;

        if (distance <= 0f)
            return 0f;

        float totalStrength = 0f;
        float totalWeight = 0f;
        var lights = _lightSystem.GetSpotLights();
        if (lights.Count == 0)
            return 0f;

        for (int dirIndex = 0; dirIndex < directions.Count; dirIndex++)
        {
            Vector2 direction = directions[dirIndex];
            if (direction.sqrMagnitude <= 0f)
                continue;

            direction = direction.normalized;

            float rayDistance = GetRayDistanceWithOcclusion(origin, direction, distance);
            if (rayDistance <= 0f)
                continue;

            int steps = Mathf.CeilToInt(rayDistance / Mathf.Max(0.01f, glareSampleStep));
            if (glareMaxSamples > 0)
                steps = Mathf.Min(steps, glareMaxSamples);
            steps = Mathf.Max(1, steps);

            float step = rayDistance / steps;

            for (int i = 0; i <= steps; i++)
            {
                float travel = step * i;
                Vector2 samplePos = origin + direction * travel;
                float strength = EvaluateLightStrengthAt(samplePos, lights);
                float weight = 1f - (travel / rayDistance);
                totalStrength += strength * weight;
                totalWeight += weight;
            }
        }

        return totalWeight > 0f ? totalStrength / totalWeight : 0f;
    }

    private float EvaluateLightStrengthAt(Vector2 worldPosition, IReadOnlyList<Light2D> lights)
    {
        float maxStrength = 0f;
        for (int i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            if (light == null || !light.isActiveAndEnabled)
                continue;

            if (!TrySampleLight(light, worldPosition, out LightSample sample))
                continue;

            if (useLightOcclusion && occlusionMask.value != 0)
            {
                Vector2 origin = light.transform.position;
                var hit = Physics2D.Linecast(origin, worldPosition, occlusionMask);
                if (hit.collider != null)
                    continue;
            }

            float strength = EvaluatePointLight(light, sample);
            if (strength > maxStrength)
                maxStrength = strength;
        }

        return maxStrength;
    }

    private bool TrySampleLight(Light2D light, Vector2 worldPosition, out LightSample sample)
    {
        sample = default;

        sample.OuterRadius = Mathf.Max(0f, light.pointLightOuterRadius);
        if (sample.OuterRadius <= 0f)
            return false;

        sample.ToTarget = (Vector2)worldPosition - (Vector2)light.transform.position;
        sample.Distance = sample.ToTarget.magnitude;
        if (sample.Distance > sample.OuterRadius)
            return false;

        sample.OuterAngle = light.pointLightOuterAngle;
        if (sample.OuterAngle >= 359.9f)
            return true;

        Vector2 forward = light.transform.up;
        sample.AngleToTarget = Vector2.Angle(forward, sample.ToTarget);
        if (sample.AngleToTarget > sample.OuterAngle * 0.5f)
            return false;

        return true;
    }

    private float EvaluatePointLight(Light2D light, in LightSample sample)
    {
        if (sample.OuterRadius <= 0f)
            return 0f;

        float distanceFactor = 1f;
        float innerRadius = Mathf.Max(0f, light.pointLightInnerRadius);
        if (innerRadius < sample.OuterRadius)
        {
            distanceFactor = Mathf.Clamp01(
                (sample.OuterRadius - sample.Distance) / Mathf.Max(0.0001f, sample.OuterRadius - innerRadius));
        }

        float angleFactor = 1f;
        if (sample.OuterAngle < 359.9f)
        {
            float halfOuter = sample.OuterAngle * 0.5f;
            float innerAngle = light.pointLightInnerAngle;
            float halfInner = innerAngle * 0.5f;
            if (halfInner < halfOuter)
            {
                angleFactor = Mathf.Clamp01(
                    (halfOuter - sample.AngleToTarget) / Mathf.Max(0.0001f, halfOuter - halfInner));
            }
        }

        return light.intensity * distanceFactor * angleFactor;
    }

    private float GetGlareRayDistance(float baseViewDistance)
    {
        if (maxViewDistance > 0f)
            return maxViewDistance;
        return baseViewDistance;
    }

    private void FillGlareDirections(Vector2 forward, Vector2[] buffer)
    {
        if (buffer == null || buffer.Length < 3)
            return;

        Vector2 normalizedForward = forward.sqrMagnitude > 0f ? forward.normalized : Vector2.right;
        buffer[0] = normalizedForward;

        if (glareRayAngleOffset <= 0f)
        {
            buffer[1] = normalizedForward;
            buffer[2] = normalizedForward;
            return;
        }

        buffer[1] = Rotate2D(normalizedForward, glareRayAngleOffset);
        buffer[2] = Rotate2D(normalizedForward, -glareRayAngleOffset);
    }

    private static Vector2 Rotate2D(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private float GetRayDistanceWithOcclusion(Vector2 origin, Vector2 direction, float maxDistance)
    {
        if (occlusionMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, occlusionMask);
            if (hit.collider != null)
                return hit.distance;
        }

        return maxDistance;
    }

    private float GetBlindVisibilityMultiplier(Vector2 origin)
    {
        if (!reduceVisionFromLight || !blindWhenInStrongLight || _lightSystem == null)
            return 1f;
        if (blindFullStrength <= 0f)
            return 1f;

        var lights = _lightSystem.GetSpotLights();
        if (lights.Count == 0)
            return 1f;

        float strength = EvaluateLightStrengthAt(origin, lights);
        if (strength <= blindStartStrength)
            return 1f;
        if (strength >= blindFullStrength)
            return 0f;

        float denom = Mathf.Max(0.0001f, blindFullStrength - blindStartStrength);
        float t = (strength - blindStartStrength) / denom;
        return Mathf.Lerp(1f, 0f, t);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawVision();
    }

    void OnDrawGizmosSelected()
    {
        DrawVision();
    }

    void DrawVision()
    {
        if (!drawGizmos || !isActiveAndEnabled)
            return;

        Vector3 pos = transform.position;
        Vector3 forward = transform.localScale.x >= 0 ? Vector3.right : Vector3.left;

        DrawVisionCone(pos, forward, maxViewAngle, maxViewDistance, maxGizmoColor);

        float effectiveViewAngle = GetEffectiveViewAngle();
        float effectiveViewDistance = GetEffectiveViewDistance();
        if (Application.isPlaying)
            effectiveViewDistance = GetEffectiveViewDistanceWithGlare();
        DrawVisionCone(pos, forward, effectiveViewAngle, effectiveViewDistance, gizmoColor);

        if (_lastSeenPosition.HasValue)
        {
            Gizmos.color = lastSeenColor;
            Gizmos.DrawSphere(_lastSeenPosition.Value, lastSeenMarkerRadius);
        }

        DrawGlareRays();
    }

    private void DrawVisionCone(Vector3 pos, Vector3 forward, float angle, float distance, Color color)
    {
        if (angle <= 0f || distance <= 0f)
            return;

        Gizmos.color = color;

        float halfAngle = angle * 0.5f;
        Vector3 leftDir = Quaternion.AngleAxis(-halfAngle, Vector3.forward) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(halfAngle, Vector3.forward) * forward;

        Gizmos.DrawLine(pos, pos + leftDir * distance);
        Gizmos.DrawLine(pos, pos + rightDir * distance);

        int segments = 30;
        Vector3 prevPoint = pos + leftDir * distance;

        for (int i = 1; i <= segments; i++)
        {
            float segAngle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            Vector3 dir = Quaternion.AngleAxis(segAngle, Vector3.forward) * forward;
            Vector3 point = pos + dir * distance;

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    private void DrawGlareRays()
    {
        if (!drawGlareRays || !Application.isPlaying)
            return;

        Vector2 origin = transform.position;
        Vector2 forward = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        float distance = GetGlareRayDistance(GetEffectiveViewDistance());
        if (distance <= 0f)
            return;

        FillGlareDirections(forward, _glareDirections);

        IReadOnlyList<Light2D> lights = _lightSystem != null ? _lightSystem.GetSpotLights() : null;

        for (int dirIndex = 0; dirIndex < _glareDirections.Length; dirIndex++)
        {
            Vector2 direction = _glareDirections[dirIndex];
            if (direction.sqrMagnitude <= 0f)
                continue;

            direction = direction.normalized;

            float rayDistance = GetRayDistanceWithOcclusion(origin, direction, distance);
            if (rayDistance <= 0f)
                continue;

            int steps = Mathf.CeilToInt(rayDistance / Mathf.Max(0.01f, glareSampleStep));
            if (glareMaxSamples > 0)
                steps = Mathf.Min(steps, glareMaxSamples);
            steps = Mathf.Max(1, steps);

            float step = rayDistance / steps;

            Gizmos.color = glareRayMinColor;
            Gizmos.DrawLine(origin, origin + direction * rayDistance);

            for (int i = 0; i <= steps; i++)
            {
                float travel = step * i;
                Vector2 samplePos = origin + direction * travel;

                float strength = 0f;
                if (lights != null && lights.Count > 0)
                    strength = EvaluateLightStrengthAt(samplePos, lights);

                float t = maxGlareStrength > 0f ? Mathf.Clamp01(strength / maxGlareStrength) : 0f;
                Gizmos.color = Color.Lerp(glareRayMinColor, glareRayMaxColor, t);
                Gizmos.DrawSphere(samplePos, glareSampleGizmoRadius);
            }
        }
    }
#endif
}

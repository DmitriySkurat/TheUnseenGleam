using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AgentLightSensor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Light Glare")]
    [SerializeField] private bool reduceVisionFromLight = true;
    [SerializeField, Min(0.01f)] private float glareSampleStep = 0.25f;
    [SerializeField, Min(1)] private int glareMaxSamples = 24;
    [SerializeField, Min(0f)] private float maxGlareStrength = 2f;
    [SerializeField, Range(0f, 1f)] private float minGlareVisibilityMultiplier = 0.3f;
    [SerializeField, Range(0f, 45f)] private float glareRayAngleOffset = 5f;
    [SerializeField, Min(0f)] private float glareStrengthEpsilon = 0.01f;
    [SerializeField, Range(-1f, 1f)] private float glareRelevantDotThreshold = 0.3f;
    [SerializeField] private bool blindWhenInStrongLight = true;
    [SerializeField, Min(0f)] private float blindStartStrength = 1f;
    [SerializeField, Min(0f)] private float blindFullStrength = 2f;

    [Header("Debug Glare Rays")]
    [SerializeField] private bool drawGlareRays = true;
    [SerializeField] private Color glareRayMinColor = new Color(0.2f, 1f, 0.4f, 0.7f);
    [SerializeField] private Color glareRayMaxColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    [SerializeField, Min(0f)] private float glareSampleGizmoRadius = 0.05f;

    private LightSystem _lightSystem;
    private readonly Vector2[] _glareDirections = new Vector2[3];
    private readonly List<Light2D> _relevantLights = new List<Light2D>(16);

    private struct LightSample
    {
        public Vector2 ToTarget;
        public float Distance;
        public float OuterRadius;
        public float OuterAngle;
        public float AngleToTarget;
    }

    public void Initialize()
    {
        _lightSystem = Services.Get<LightSystem>();
    }

    public float GetVisibilityMultiplier(Vector2 origin, Vector2 forward, float rayDistance, LayerMask occlusionMask)
    {
        if (!reduceVisionFromLight || rayDistance <= 0f)
            return 1f;

        EnsureLightSystem();
        if (_lightSystem == null)
            return 1f;

        FillGlareDirections(forward, _glareDirections);
        float glareStrength = EvaluateGlareStrength(origin, _glareDirections, rayDistance, occlusionMask);
        float glareMultiplier = GetGlareVisibilityMultiplier(glareStrength);
        float blindMultiplier = GetBlindVisibilityMultiplier(origin);
        return glareMultiplier * blindMultiplier;
    }

    public void DrawGlareRays(Vector2 origin, Vector2 forward, float distance, LayerMask occlusionMask)
    {
        if (!drawGlareRays || !Application.isPlaying)
            return;

        if (distance <= 0f)
            return;

        EnsureLightSystem();
        if (_lightSystem == null)
            return;

        FillGlareDirections(forward, _glareDirections);

        for (int dirIndex = 0; dirIndex < _glareDirections.Length; dirIndex++)
        {
            Vector2 direction = _glareDirections[dirIndex];
            if (direction.sqrMagnitude <= 0f)
                continue;

            direction = direction.normalized;

            float rayDistance = GetRayDistanceWithOcclusion(origin, direction, distance, occlusionMask);
            if (rayDistance <= 0f)
                continue;

            FillRelevantLights(origin, direction, rayDistance, _relevantLights);
            IReadOnlyList<Light2D> lights = _relevantLights;

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
                if (lights.Count > 0)
                    strength = EvaluateLightStrengthAt(samplePos, lights);

                float t = maxGlareStrength > 0f ? Mathf.Clamp01(strength / maxGlareStrength) : 0f;
                Gizmos.color = Color.Lerp(glareRayMinColor, glareRayMaxColor, t);
                Gizmos.DrawSphere(samplePos, glareSampleGizmoRadius);
            }
        }
    }

    private void EnsureLightSystem()
    {
        if (_lightSystem == null)
            _lightSystem = Services.Get<LightSystem>();
    }

    private float GetGlareVisibilityMultiplier(float glareStrength)
    {
        if (maxGlareStrength <= 0f)
            return 1f;

        float clampedMin = Mathf.Clamp01(minGlareVisibilityMultiplier);
        float t = Mathf.Clamp01(glareStrength / maxGlareStrength);
        return Mathf.Lerp(1f, clampedMin, t);
    }

    private float EvaluateGlareStrength(Vector2 origin, IReadOnlyList<Vector2> directions, float distance, LayerMask occlusionMask)
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

            float rayDistance = GetRayDistanceWithOcclusion(origin, direction, distance, occlusionMask);
            if (rayDistance <= 0f)
                continue;

            FillRelevantLights(origin, direction, rayDistance, _relevantLights);
            if (_relevantLights.Count == 0)
                continue;

            int steps = Mathf.CeilToInt(rayDistance / Mathf.Max(0.01f, glareSampleStep));
            if (glareMaxSamples > 0)
                steps = Mathf.Min(steps, glareMaxSamples);
            steps = Mathf.Max(1, steps);

            float step = rayDistance / steps;
            float lastStrength = 0f;
            bool hasLastStrength = false;
            bool reuseNext = false;

            for (int i = 0; i <= steps; i++)
            {
                float travel = step * i;
                Vector2 samplePos = origin + direction * travel;
                float strength;
                if (reuseNext)
                {
                    strength = lastStrength;
                    reuseNext = false;
                }
                else
                {
                    strength = EvaluateLightStrengthAt(samplePos, _relevantLights);
                    if (hasLastStrength && glareStrengthEpsilon > 0f &&
                        Mathf.Abs(strength - lastStrength) < glareStrengthEpsilon)
                    {
                        reuseNext = true;
                    }

                    lastStrength = strength;
                    hasLastStrength = true;
                }

                float weight = 1f - (travel / rayDistance);
                totalStrength += strength * weight;
                totalWeight += weight;

                if (maxGlareStrength > 0f && totalWeight > 0f &&
                    (totalStrength / totalWeight) >= maxGlareStrength)
                {
                    return maxGlareStrength;
                }
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
        float forwardMag = forward.magnitude;
        if (forwardMag <= 0f || sample.Distance <= 0f)
            return false;

        float cosHalfAngle = Mathf.Cos(sample.OuterAngle * 0.5f * Mathf.Deg2Rad);
        float dot = Vector2.Dot(forward, sample.ToTarget) / (forwardMag * sample.Distance);
        if (dot < cosHalfAngle)
            return false;

        dot = Mathf.Clamp(dot, -1f, 1f);
        sample.AngleToTarget = Mathf.Acos(dot) * Mathf.Rad2Deg;
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

    private float GetRayDistanceWithOcclusion(Vector2 origin, Vector2 direction, float maxDistance, LayerMask occlusionMask)
    {
        if (occlusionMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, occlusionMask);
            if (hit.collider != null)
                return hit.distance;
        }

        return maxDistance;
    }

    private void FillRelevantLights(Vector2 origin, Vector2 direction, float distance, List<Light2D> result)
    {
        result.Clear();
        var lights = _lightSystem.GetSpotLights();
        if (lights.Count == 0)
            return;

        float maxDot = Mathf.Clamp(glareRelevantDotThreshold, -1f, 1f);

        for (int i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            if (light == null || !light.isActiveAndEnabled)
                continue;

            float maxRange = Mathf.Max(0f, light.pointLightOuterRadius);
            if (maxRange <= 0f)
                continue;

            Vector2 toLight = (Vector2)light.transform.position - origin;
            float toLightSqr = toLight.sqrMagnitude;
            if (toLightSqr <= 0.0001f)
            {
                result.Add(light);
                continue;
            }

            float maxReach = distance + maxRange;
            if (toLightSqr > maxReach * maxReach)
                continue;

            float toLightMag = Mathf.Sqrt(toLightSqr);
            float dot = Vector2.Dot(direction, toLight / toLightMag);
            if (dot < maxDot)
                continue;

            result.Add(light);
        }
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
}

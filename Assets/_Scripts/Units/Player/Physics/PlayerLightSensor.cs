using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightSensor : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;

    public float CurrentStrength { get; protected set; }

    [Header("References")]
    [SerializeField] private Utility.Logger _logger;

    [Header("Sampling")]
    [SerializeField, Min(0f)] private float sampleInterval = 0.05f;
    [SerializeField, Min(0f)] private float lightsRefreshInterval = 0.5f;

    [Header("Evaluation")]
    [SerializeField] private bool useDistanceFalloff = true;
    [SerializeField] private bool useOcclusion = true;
    [SerializeField] private LayerMask occlusionMask;

    private LightSystem _lightSystem;
    private float _nextSampleTime;
    private float _nextRefreshTime;

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

        if (_lightSystem != null)
        {
            _logger.Log("LightSystem got in LightExposureSenser", this);
            _logger.Log($"Count of spotlights: {_lightSystem.GetSpotLights().Count}", this);
        }
    }
    
    public void Dispose()
    {
        
    }

    private void Update()
    {
        if (_lightSystem == null)
            return;

        if (Time.time >= _nextRefreshTime && lightsRefreshInterval > 0f)
        {
            _lightSystem.RefreshSpotLightsCache();
            _nextRefreshTime = Time.time + lightsRefreshInterval;
        }

        if (Time.time >= _nextSampleTime)
        {
            _nextSampleTime = Time.time + sampleInterval;
            CurrentStrength = EvaluateLightStrength(transform.position);
        }
    }

    private float EvaluateLightStrength(Vector2 worldPosition)
    {
        if (!TryGetNearestLight(worldPosition, out Light2D nearestLight, out LightSample nearestSample))
            return 0f;

        if (useOcclusion && occlusionMask.value != 0)
        {
            Vector2 origin = nearestLight.transform.position;
            Vector2 target = worldPosition;
            var hit = Physics2D.Linecast(origin, target, occlusionMask);
            if (hit.collider != null)
                return 0f;
        }

        float strength = EvaluatePointLight(nearestLight, nearestSample);

        //_logger.Log($"NearestLightStrength: {strength}", this);

        return strength;
    }

    private bool TryGetNearestLight(Vector2 worldPosition, out Light2D nearestLight, out LightSample nearestSample)
    {
        nearestLight = null;
        nearestSample = default;
        float nearestDistance = float.PositiveInfinity;

        foreach (var light in _lightSystem.GetSpotLights())
        {
            if (light == null || !light.isActiveAndEnabled)
                continue;

            if (!TrySampleLight(light, worldPosition, out LightSample sample))
                continue;

            if (sample.Distance < nearestDistance)
            {
                nearestDistance = sample.Distance;
                nearestLight = light;
                nearestSample = sample;
            }
        }

        return nearestLight != null;
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
        if (useDistanceFalloff)
        {
            float innerRadius = Mathf.Max(0f, light.pointLightInnerRadius);
            distanceFactor = innerRadius >= sample.OuterRadius
                ? 1f
                : Mathf.Clamp01(
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
}

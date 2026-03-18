using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightExposureSensor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.GameplayCore;
    
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

    private float _lightStrength;
    private float _lightFactor;
    private float _nextSampleTime;
    private float _nextRefreshTime;


    public void Initialize()
    {
        _lightSystem = Services.Get<LightSystem>();
        
        if (_lightSystem != null)
        {
            _logger.Log("LightSystem got in LightExposureSenser", this);
            _logger.Log($"Count of spotlights: {_lightSystem.GetSpotLights().Count}", this);
        }
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

        if (Time.time < _nextSampleTime)
            return;

        _nextSampleTime = Time.time + sampleInterval;

        float targetStrength = EvaluateLightStrength(transform.position);
        _lightStrength = targetStrength;
    }

    private float EvaluateLightStrength(Vector3 worldPosition)
    {
        float maxStrength = 0f;

        foreach (var light in _lightSystem.GetSpotLights())
        {
            if (light == null || !light.isActiveAndEnabled)
                continue;

            if (useOcclusion && occlusionMask.value != 0)
            {
                Vector2 origin = light.transform.position;
                Vector2 target = worldPosition;
                var hit = Physics2D.Linecast(origin, target, occlusionMask);
                if (hit.collider != null)
                    continue;
            }

            float strength = EvaluatePointLight(light, worldPosition);

            if (strength > maxStrength)
            {
                maxStrength = strength;
            }
        }
        
        _logger.Log($"MaxStrenght: {maxStrength}" ,this);

        return maxStrength;
    }

    private float EvaluatePointLight(Light2D light, Vector3 worldPosition)
    {
        float outerRadius = Mathf.Max(0f, light.pointLightOuterRadius);
        if (outerRadius <= 0f)
            return 0f;

        Vector2 toTarget = (Vector2)(worldPosition - light.transform.position);
        float distance = toTarget.magnitude;
        if (distance > outerRadius)
            return 0f;

        float distanceFactor = 1f;
        if (useDistanceFalloff)
        {
            float innerRadius = Mathf.Max(0f, light.pointLightInnerRadius);
            distanceFactor = innerRadius >= outerRadius
                ? 1f
                : Mathf.Clamp01(
                    (outerRadius - distance) / Mathf.Max(0.0001f, outerRadius - innerRadius));
        }

        float angleFactor = 1f;
        
        float outerAngle = light.pointLightOuterAngle;
        if (outerAngle < 359.9f)
        {
            Vector2 forward = light.transform.up;

            float angleToTarget = Vector2.Angle(forward, toTarget);
            float halfOuter = outerAngle * 0.5f;
            if (angleToTarget > halfOuter)
                return 0f;

            float innerAngle = light.pointLightInnerAngle;
            float halfInner = innerAngle * 0.5f;
            if (halfInner < halfOuter)
            {
                angleFactor = Mathf.Clamp01(
                    (halfOuter - angleToTarget) / Mathf.Max(0.0001f, halfOuter - halfInner));
            }
        }
        

        return light.intensity * distanceFactor * angleFactor;
    }
}

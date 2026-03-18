using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightExposureSensor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.GameplayCore;
    
    [Header("References")]
    [SerializeField] private Utility.Logger _logger;


    [Header("Exposure Settings")]
    [SerializeField, Min(0.01f)] private float maxIntensityForFullLight = 1f;
    [SerializeField, Min(0f)] private float litThreshold = 0.05f;
    [SerializeField, Min(0f)] private float smoothSpeed = 8f;

    [Header("Evaluation")]
    [SerializeField] private bool useDistanceFalloff = true;
    [SerializeField] private bool useSpotAngle = true;
    [SerializeField] private bool useOcclusion = true;
    [SerializeField] private LayerMask occlusionMask;

    private LightSystem _lightSystem;

    private float _lightStrength;
    private float _lightFactor;

    public float LightStrength => _lightStrength;
    public float LightFactor => _lightFactor;
    public bool IsLit => _lightStrength >= litThreshold;

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
        if (_lightSystem == null) return;

        float targetStrength = EvaluateLightStrength(transform.position);
        float targetFactor = maxIntensityForFullLight > 0f
            ? Mathf.Clamp01(targetStrength / maxIntensityForFullLight)
            : 0f;

        _lightStrength = targetStrength;

        if (smoothSpeed > 0f)
        {
            _lightFactor = Mathf.Lerp(_lightFactor, targetFactor, Time.deltaTime * smoothSpeed);
        }
        else
        {
            _lightFactor = targetFactor;
        }
    }

    private float EvaluateLightStrength(Vector2 point)
    {
        float maxStrength = 0f;
        var lights = _lightSystem.GetSpotLights();

        for (int i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            if (light == null || light.intensity <= 0f) continue;

            Vector2 origin = light.transform.position;
            Vector2 toPoint = point - origin;
            float dist = toPoint.magnitude;

            if (dist > light.pointLightOuterRadius) continue;
            if (useSpotAngle && !IsInsideSpotCone(light, toPoint)) continue;
            if (useOcclusion && occlusionMask.value != 0)
            {
                var hit = Physics2D.Linecast(origin, point, occlusionMask);
                if (hit.collider != null) continue;
            }

            float strength = light.intensity;

            if (useDistanceFalloff)
            {
                strength *= GetDistanceFactor(light, dist);
            }

            if (useSpotAngle)
            {
                strength *= GetAngleFactor(light, toPoint);
            }

            if (strength > maxStrength)
            {
                maxStrength = strength;
            }
        }
        
        _logger.Log($"MaxStrenght: {maxStrength}" ,this);

        return maxStrength;
    }

    private static float GetDistanceFactor(Light2D light, float distance)
    {
        float inner = light.pointLightInnerRadius;
        float outer = light.pointLightOuterRadius;
        if (outer <= inner) return 1f;

        float t = Mathf.Clamp01((distance - inner) / (outer - inner));
        float factor = 1f - t;
        return factor * factor;
    }

    private static bool IsInsideSpotCone(Light2D light, Vector2 toPoint)
    {
        if (light.pointLightOuterAngle >= 360f) return true;
        Vector2 forward = light.transform.up;
        float angle = Vector2.Angle(forward, toPoint);
        return angle <= light.pointLightOuterAngle * 0.5f;
    }

    private static float GetAngleFactor(Light2D light, Vector2 toPoint)
    {
        if (light.pointLightOuterAngle >= 360f) return 1f;
        Vector2 forward = light.transform.up;
        float angle = Vector2.Angle(forward, toPoint);
        float half = Mathf.Max(0.001f, light.pointLightOuterAngle * 0.5f);
        return 1f - Mathf.Clamp01(angle / half);
    }
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Маркер для источников света, которые используются только визуально.
/// LightSystem исключает такие источники из кэша спотлайтов,
/// поэтому они не влияют на PlayerLightSensor, AgentLightSensor, MagicalMirror и т.п.
///
/// Опционально: уменьшает интенсивность пропорционально освещённости из PlayerLightSensor.
/// </summary>
[DisallowMultipleComponent]
public class CosmeticLight : MonoBehaviour
{
    [Header("Proximity Dimming")]
    [SerializeField] private bool enableProximityDimming = true;
    [SerializeField, Min(0.01f)] private float fullDimAtStrength = 1f;
    [SerializeField, Range(0f, 1f)] private float minIntensityMultiplier = 0f;
    [SerializeField, Min(0f)] private float dimmingSpeed = 3f;

    private Light2D _light;
    private float _baseIntensity;
    private float _currentMultiplier = 1f;
    private PlayerLightSensor _sensor;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
        if (_light != null)
            _baseIntensity = _light.intensity;

        _sensor = GetComponentInParent<PlayerLightSensor>();
    }

    private void Update()
    {
        if (!enableProximityDimming || _light == null || _sensor == null)
            return;

        float t = Mathf.Clamp01(_sensor.CurrentStrength / fullDimAtStrength);
        float targetMultiplier = Mathf.Lerp(1f, minIntensityMultiplier, t);

        _currentMultiplier = Mathf.MoveTowards(_currentMultiplier, targetMultiplier, dimmingSpeed * Time.deltaTime);
        _light.intensity = _baseIntensity * _currentMultiplier;
    }
}

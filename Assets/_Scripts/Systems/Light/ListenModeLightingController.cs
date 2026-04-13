using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// При зажатом ПКМ ("прислушаться") выравнивает свет в сцене:
/// плавно гасит точечные источники и поднимает глобальный свет.
/// </summary>
public class ListenModeLightingController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.PostProcessing;

    [Header("Global Light")]
    [SerializeField, Range(0f, 2f)] private float listenGlobalIntensity = 1f;
    [SerializeField] private float smoothSpeed = 4f;

    [Header("Spot Lights")]
    [Tooltip("При этом значении intensity источник отключается (избегаем артефактов)")]
    [SerializeField, Range(0f, 2f)] private float disableThreshold = 0.05f;

    private CameraFollow _cameraFollow;
    private LightSystem _lightSystem;

    private float _baseGlobalIntensity;
    private Color _baseGlobalColor;
    private float _currentGlobalIntensity;

    private struct SpotLightSnapshot
    {
        public Light2D light;
        public float originalIntensity;
    }
    private readonly List<SpotLightSnapshot> _snapshots = new();

    public void Initialize()
    {
        _cameraFollow = Services.Get<CameraFollow>();
        _lightSystem  = Services.Get<LightSystem>();

        _baseGlobalIntensity = _lightSystem.GlobalLightIntensity;
        _baseGlobalColor     = _lightSystem.GlobalLightColor;
        _currentGlobalIntensity = _baseGlobalIntensity;

        CaptureSpotLights();
    }

    private void CaptureSpotLights()
    {
        _snapshots.Clear();
        foreach (var light in _lightSystem.GetSpotLights())
        {
            if (light == null) continue;
            _snapshots.Add(new SpotLightSnapshot
            {
                light             = light,
                originalIntensity = light.intensity
            });
        }
    }

    private void Update()
    {
        if (_lightSystem == null || _cameraFollow == null) return;

        bool isListening = _cameraFollow.IsLookingAround();

        UpdateGlobalLight(isListening);
        UpdateSpotLights(isListening);
    }

    private void UpdateGlobalLight(bool isListening)
    {
        float target = isListening ? listenGlobalIntensity : _baseGlobalIntensity;
        _currentGlobalIntensity = Mathf.Lerp(_currentGlobalIntensity, target, Time.deltaTime * smoothSpeed);
        _lightSystem.UpdateGlobalLight(_currentGlobalIntensity, _baseGlobalColor);
    }

    private void UpdateSpotLights(bool isListening)
    {
        foreach (var snap in _snapshots)
        {
            var light = snap.light;
            if (light == null) continue;

            if (isListening)
            {
                if (!light.enabled) continue;

                light.intensity = Mathf.Lerp(light.intensity, 0f, Time.deltaTime * smoothSpeed);

                if (light.intensity <= disableThreshold)
                {
                    light.intensity = 0f;
                    light.enabled   = false;
                }
            }
            else
            {
                // Восстанавливаем: сначала включаем, потом плавно возвращаем intensity
                if (!light.enabled)
                {
                    light.intensity = _currentGlobalIntensity;
                    light.enabled   = true;
                }

                light.intensity = Mathf.Lerp(light.intensity, snap.originalIntensity, Time.deltaTime * smoothSpeed);
            }
        }
    }
}

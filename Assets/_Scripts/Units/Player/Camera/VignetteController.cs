using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class VignetteController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.PostProcessing;


    [Header("References")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private LightExposureSensor lightExposureSensor;
    
    
    [Header("Intensity Settings")]
    [SerializeField] private float defaultIntensity = 0.5f;
    [SerializeField] private float aimIntensity = 0.25f;

    [Header("Light Influence")]
    [SerializeField, Min(0f)] private float maxLightStrength = 1f;
    [SerializeField, Range(0f, 1f)] private float minIntensityInLight = 0.05f;
    [SerializeField, Range(0f, 1f)] private float lightInfluence = 1f;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;
    
    private Volume _volume;
    private Vignette _vignette;


    private float currentIntensity;
    
    
    public void Initialize()
    {
        _volume = GetComponent<Volume>();

        if (!_volume.profile.TryGet(out _vignette))
        {
            Debug.LogError("Vignette effect not found in Volume Profile!");
        }
        else
        {
            currentIntensity = defaultIntensity;
            _vignette.intensity.value = defaultIntensity;
        }
    }

    private void Update()
    {
        //Debug.Log($"cameraFollow: {cameraFollow.GetHashCode()} and vignette: {_vignette.GetHashCode()}");
        if (_vignette == null || cameraFollow == null) return;

        bool isLooking = cameraFollow.IsLookingAround();
        float baseIntensity = isLooking ? aimIntensity : defaultIntensity;
        float targetIntensity = baseIntensity;

        if (lightExposureSensor != null && maxLightStrength > 0f && lightInfluence > 0f)
        {
            float normalizedLight = Mathf.Clamp01(lightExposureSensor.CurrentStrength / maxLightStrength);
            float lightFactor = Mathf.Clamp01(normalizedLight * lightInfluence);
            targetIntensity = Mathf.Lerp(baseIntensity, minIntensityInLight, lightFactor);
        }

        targetIntensity = Mathf.Clamp01(targetIntensity);

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        _vignette.intensity.value = currentIntensity;
    }
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class VignetteController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.PostProcessing;
    
    [Header("Intensity Settings")]
    [SerializeField] private float defaultIntensity = 0.5f;
    [SerializeField] private float aimIntensity = 0.25f;

    [Header("Light Influence")]
    [SerializeField, Min(0f)] private float maxLightStrength = 1f;
    [SerializeField, Range(0f, 1f)] private float minIntensityInLight = 0.05f;
    [SerializeField, Range(0f, 1f)] private float lightInfluence = 1f;
    [SerializeField, Range(0f, 5f)] private float lightDecreaseDelay = 0.5f;
    
    [Header("Limits")]
    [SerializeField, Range(0f, 1f)] private float minIntensityClamp = 0.05f;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;
    
    private CameraFollow _cameraFollow;
    private LightExposureSensor _lightExposureSensor;
        
    private Volume _volume;
    private Vignette _vignette;
    
    private PlayerContext _ctx;


    private float currentIntensity;
    private float _timeInLight;
    
    
    public void Initialize()
    {
        _volume = GetComponent<Volume>();
        
        _cameraFollow = Services.Get<CameraFollow>();
        
        _lightExposureSensor = Services.Get<PlayerContext>().lightSensor;
        
        

        if (!_volume.profile.TryGet(out _vignette))
        {
            Debug.LogError("Vignette effect not found in Volume Profile!");
        }
        else
        {
            currentIntensity = Mathf.Clamp(defaultIntensity, minIntensityClamp, 1f);
            _vignette.intensity.value = currentIntensity;
        }

        _timeInLight = 0f;
    }

    private void Update()
    {
        //Debug.Log($"cameraFollow: {cameraFollow.GetHashCode()} and vignette: {_vignette.GetHashCode()}");
        if (_vignette == null || _cameraFollow == null) return;

        bool isLooking = _cameraFollow.IsLookingAround();
        float baseIntensity = isLooking ? aimIntensity : defaultIntensity;
        float targetIntensity = baseIntensity;

        if (_lightExposureSensor != null && maxLightStrength > 0f && lightInfluence > 0f)
        {
            float normalizedLight = Mathf.Clamp01(_lightExposureSensor.CurrentStrength / maxLightStrength);
            float lightFactor = Mathf.Clamp01(normalizedLight * lightInfluence);

            if (lightFactor > 0f)
                _timeInLight += Time.deltaTime;
            else
                _timeInLight = 0f;

            if (_timeInLight >= lightDecreaseDelay)
                targetIntensity = Mathf.Lerp(baseIntensity, minIntensityInLight, lightFactor);
        }
        else
        {
            _timeInLight = 0f;
        }

        targetIntensity = Mathf.Clamp(targetIntensity, minIntensityClamp, 1f);

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        _vignette.intensity.value = currentIntensity;
    }
}

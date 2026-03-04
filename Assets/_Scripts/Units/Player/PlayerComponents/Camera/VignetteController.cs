using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class VignetteController : MonoBehaviour, IInitializable
{
    [Header("References")]
    [SerializeField] private CameraFollow cameraFollow;
    
    public InitializationOrder Order => InitializationOrder.PostProcessing;
    
    [Header("Intensity Settings")]
    [SerializeField] private float defaultIntensity = 0.5f;
    [SerializeField] private float aimIntensity = 0.25f;
    
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

    // private void Awake()
    // {
    //     _volume = GetComponent<Volume>();

    //     if (!_volume.profile.TryGet(out _vignette))
    //     {
    //         Debug.LogError("Vignette effect not found in Volume Profile!");
    //     }
    //     else
    //     {
    //         currentIntensity = defaultIntensity;
    //         _vignette.intensity.value = defaultIntensity;
    //     }
    // }

    private void Update()
    {
        if (_vignette == null || cameraFollow == null) return;

        // Vignette reduces only when camera is looking around
        bool isLooking = cameraFollow.IsLookingAround();
        float targetIntensity = isLooking ? aimIntensity : defaultIntensity;

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        _vignette.intensity.value = currentIntensity;
    }
}
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class VignetteController : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;

    private Volume volume;
    private Vignette vignette;

    [SerializeField] private float defaultIntensity = 0.5f;
    [SerializeField] private float aimIntensity = 0.25f;
    [SerializeField] private float smoothSpeed = 5f;

    private float currentIntensity;

    private void Awake()
    {
        volume = GetComponent<Volume>();

        if (!volume.profile.TryGet(out vignette))
        {
            Debug.LogError("Vignette effect not found in Volume Profile!");
        }
        else
        {
            currentIntensity = defaultIntensity;
            vignette.intensity.value = defaultIntensity;
        }
    }

    private void Update()
    {
        if (vignette == null || cameraFollow == null) return;

        // Vignette reduces only when camera is looking around
        bool isLooking = cameraFollow.IsLookingAround();
        float targetIntensity = isLooking ? aimIntensity : defaultIntensity;

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        vignette.intensity.value = currentIntensity;
    }
}
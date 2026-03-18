using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSystem : MonoBehaviour, ISceneService
{    
    [Header("References")]
    [SerializeField] private Light2D globalLightPrefab;
    [SerializeField] private Utility.Logger _logger;

    [Header("Global Light Settings")]
    [SerializeField, Range(0f, 2f)] private float globalLightIntensity = 0.7f;
    [SerializeField] private Color globalLightColor = Color.white;

    [Header("Spot Light Cache")]
    [SerializeField] private bool includeInactiveSpotLights = true;

    private Light2D _globalLightInstance;
    private readonly List<Light2D> _spotLights = new List<Light2D>(64);
    
    public async Task InitializeAsync()
    {
        if (globalLightPrefab != null)
        {
            _globalLightInstance = Instantiate(globalLightPrefab);
            _globalLightInstance.intensity = globalLightIntensity;
            _globalLightInstance.color = globalLightColor;
        }

        RefreshSpotLightsCache();
        
        _logger.Log("LightSystem initialized" ,this);
        
        await Task.CompletedTask;
    }
    
    public void UpdateGlobalLight(float intensity, Color color)
    {
        if (_globalLightInstance != null)
        {
            _globalLightInstance.intensity = intensity;
            _globalLightInstance.color = color;
        }
    }

    public IReadOnlyList<Light2D> GetSpotLights()
    {
        PruneDestroyedSpotLights();
        return _spotLights;
    }

    public void RefreshSpotLightsCache()
    {
        _spotLights.Clear();

        var lights = FindObjectsByType<Light2D>(
            includeInactiveSpotLights ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            var light = lights[i];
            if (IsSpotLight2D(light))
            {
                _spotLights.Add(light);
            }
        }
    }

    public bool RegisterSpotLight(Light2D light)
    {
        if (!IsSpotLight2D(light)) return false;
        if (_spotLights.Contains(light)) return false;
        _spotLights.Add(light);
        return true;
    }

    public bool UnregisterSpotLight(Light2D light)
    {
        if (light == null) return false;
        return _spotLights.Remove(light);
    }

    private void PruneDestroyedSpotLights()
    {
        for (int i = _spotLights.Count - 1; i >= 0; i--)
        {
            if (_spotLights[i] == null)
            {
                _spotLights.RemoveAt(i);
            }
        }
    }

    private static bool IsSpotLight2D(Light2D light)
    {
        if (light == null) return false;
        // Unity uses LightType.Point for point/spot 2D lights; global lights are excluded.
        return light.lightType == Light2D.LightType.Point;
    }
}

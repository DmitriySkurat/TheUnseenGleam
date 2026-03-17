using UnityEngine;
using System.Threading.Tasks;

public class LightSystem : MonoBehaviour, ISceneService
{
    public InitializationOrder Order => InitializationOrder.SceneServices;
    
    
    [Header("Global Light Settings")]
    [SerializeField, Range(0f, 2f)] private float globalLightIntensity = 0.7f;
    [SerializeField] private Color globalLightColor = Color.white;

    public async Task InitializeAsync()
    {
        
        RenderSettings.ambientIntensity = globalLightIntensity;
        RenderSettings.ambientLight = globalLightColor;
        
        await Task.CompletedTask;
    }



    public void UpdateGlobalLight(float intensity, Color color)
    {
        RenderSettings.ambientIntensity = intensity;
        RenderSettings.ambientLight = color;
    }
}
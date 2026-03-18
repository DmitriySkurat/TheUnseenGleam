using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class GameplaySceneServiceRegistry : MonoBehaviour
{
    
    [SerializeField] private PlayerScriptableStats stats;
    
    [SerializeField] private NoiseSystem noisePrefab;
    [SerializeField] private LightSystem lightPrefab;
    [SerializeField] private CameraFollow cameraPrefab;

    public async Task InitializeAsync()
    {
        var services = new List<ISceneService>();
        
        Debug.Log("SceneServicesRegistrar Initialize()");
    
        var playerContext = new PlayerContext();
        playerContext.stats = stats;
        Services.Register(playerContext);
        
        
        Instantiate(cameraPrefab);
        

        var noise = Instantiate(noisePrefab);
        var light = Instantiate(lightPrefab);
        
        Services.Register(noise);
        Services.Register(light);
        
        services.Add(noise);
        services.Add(light);
        
        // Здесь же можно зарегистрировать другие сценовые сервисы
        // Services.Register<InventorySystem>(new InventorySystem());
        
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }
    
    public void Dispose()
    {
        Services.Unregister<PlayerContext>();
        Services.Unregister<NoiseSystem>();
        Services.Unregister<LightSystem>();
    }
}

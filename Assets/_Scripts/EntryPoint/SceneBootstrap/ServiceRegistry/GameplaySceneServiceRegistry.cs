using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class GameplaySceneServiceRegistry : MonoBehaviour
{
    
    [SerializeField] private PlayerScriptableStats stats;
    [SerializeField] private NoiseScriptableStats noiseStats;
    
    [SerializeField] private NoiseSystem noisePrefab;
    [SerializeField] private LightSystem lightPrefab;
    [SerializeField] private CameraFollow cameraPrefab;
    [SerializeField] private AgentAlertSystem agentAlertSystemPrefab;

    public async Task InitializeAsync()
    {
        var services = new List<ISceneService>();
        
        Debug.Log("SceneServicesRegistrar Initialize()");
    
        var playerContext = new PlayerContext();
        playerContext.stats = stats;
        playerContext.noiseStats = noiseStats;
        Services.Register(playerContext);
        
        
        var camera = Instantiate(cameraPrefab);
        Services.Register(camera);
        


        var noise = Instantiate(noisePrefab);
        var light = Instantiate(lightPrefab);
        var alert = Instantiate(agentAlertSystemPrefab);
        var sessionEndHandler = new GameObject("SessionEndHandler").AddComponent<SessionEndHandler>();
        

        Services.Register(noise);
        Services.Register(light);
        Services.Register(alert);
        Services.Register(sessionEndHandler);

        services.Add(noise);
        services.Add(light);
        services.Add(alert);
        
        // Здесь же можно зарегистрировать другие сценовые сервисы
        // Services.Register<InventorySystem>(new InventorySystem());
        
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }

    public void Dispose()
    {
        Services.Unregister<PlayerContext>();
        Services.Unregister<NoiseSystem>();
        Services.Unregister<LightSystem>();
        Services.Unregister<CameraFollow>();
        Services.Unregister<AgentAlertSystem>();
        Services.Unregister<SessionEndHandler>();
    }

    // Временно, потом мб придумаю что-то
    void OnDestroy()
    {
        Dispose();
    }
}

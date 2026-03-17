using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class GameplaySceneServiceRegistry : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.SceneServices; 
    
    [SerializeField] private PlayerScriptableStats stats;
    
    [SerializeField] private NoiseSystem noisePrefab;
    [SerializeField] private LightSystem lightPrefab;

    public void Initialize()
    {
        Debug.Log("SceneServicesRegistrar Initialize()");
    
        var playerContext = new PlayerContext();
        playerContext.stats = stats;
        Services.Register(playerContext);
        
        var noiseSystem = Instantiate(noisePrefab);
        var lightSystem = Instantiate(lightPrefab);
        
        Services.Register(noiseSystem);
        Services.Register(lightSystem);
        
        // Здесь же можно зарегистрировать другие сценовые сервисы
        // Services.Register<InventorySystem>(new InventorySystem());
    }
    
    public async Task InitializeAsync()
    {
        DontDestroyOnLoad(gameObject);

        var services = new List<IService>();

        var noiseSystem = Instantiate(noisePrefab);
        var lightSystem = Instantiate(lightPrefab);

        DontDestroyOnLoad(noiseSystem.gameObject);
        DontDestroyOnLoad(lightSystem.gameObject);

        Services.Register(noiseSystem);
        Services.Register(lightSystem);

        services.Add(noiseSystem);
        services.Add(lightSystem);
        
        // Параллельная инициализация
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }
    
    public void Dispose()
    {
        Services.Unregister<PlayerContext>();
        Services.Unregister<NoiseSystem>();
        Services.Unregister<LightSystem>();
    }
}

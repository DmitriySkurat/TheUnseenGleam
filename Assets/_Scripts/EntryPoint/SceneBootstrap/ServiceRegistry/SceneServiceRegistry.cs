using UnityEngine;

public class SceneServiceRegistry : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.SceneServices; 

    public void Initialize()
    {
        Debug.Log("SceneServicesRegistrar Initialize()");
    
        var playerContext = new PlayerContext();
        Services.Register(playerContext);
        
        // Здесь же можно зарегистрировать другие сценовые сервисы
        // Services.Register<InventorySystem>(new InventorySystem());
    }
    
    // Не забываем очистить при выгрузке
    public void Dispose()
    {
        Services.Unregister<PlayerContext>();
    }
}
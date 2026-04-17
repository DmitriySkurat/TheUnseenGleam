using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public abstract class SceneServiceRegistry : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected Utility.Logger _logger;
        
        
    public virtual async Task InitializeAsync()
    {
        var services = new List<ISceneService>();
        
        Debug.Log("SceneServicesRegistrar Initialize()");
    
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }
    
    public virtual void Dispose()
    {
        //Services.Unregister<Service>();
    }
    
    private void OnDestroy()
    {
        Dispose();
    }
}
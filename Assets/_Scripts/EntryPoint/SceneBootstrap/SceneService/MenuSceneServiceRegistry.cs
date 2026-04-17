using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class MenuSceneServiceRegistry : SceneServiceRegistry
{
    //[SerializeField] private CameraFollow cameraPrefab;

    public override async Task InitializeAsync()
    {
        var services = new List<ISceneService>();
        
        Debug.Log("MenuServicesRegistrar Initialize()");
    
        
        // var camera = Instantiate(cameraPrefab);
        // Services.Register(camera);
        

        
        
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }
}

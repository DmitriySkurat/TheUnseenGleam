using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class MenuSceneServiceRegistry : MonoBehaviour
{
    //[SerializeField] private CameraFollow cameraPrefab;

    public async Task InitializeAsync()
    {
        var services = new List<ISceneService>();
        
        Debug.Log("MenuServicesRegistrar Initialize()");
    
        
        // var camera = Instantiate(cameraPrefab);
        // Services.Register(camera);
        

        
        
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }

    public void Dispose()
    {
        //Services.Unregister<CameraFollow>();
    }

    // Временно, потом мб придумаю что-то
    void OnDestroy()
    {
        Dispose();
    }
}

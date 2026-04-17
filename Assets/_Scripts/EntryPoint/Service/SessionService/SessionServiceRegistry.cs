using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class SessionServiceRegistry : ServiceRegistry
{
    private readonly List<GameObject> _sessionObjects = new();


    // public void Dispose()
    // {
    //     // Пример отмены регистрации:
    //     // Services.Unregister<SomeService>();

    //     foreach (var obj in _sessionObjects)
    //     {
    //         if (obj != null)
    //             Destroy(obj);
    //     }

    //     _sessionObjects.Clear();
    // }
}

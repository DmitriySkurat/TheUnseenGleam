using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public abstract class ServiceRegistry : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected Utility.Logger _logger;

    [Header("Service Prefabs")]
    [SerializeField] private List<MonoBehaviour> _servicePrefabs = new();

    [Header("Plain Prefabs")]
    [SerializeField] private List<GameObject> _plainPrefabs = new();

    [Header("Settings")]
    [SerializeField] private bool _persistAcrossScenes;

    protected readonly List<IService> _services = new();
    private readonly List<Type> _registeredServiceTypes = new();

    public virtual async Task InitializeAsync()
    {
        _logger.Log($"{GetType().Name} Initialize()", this);

        foreach (var prefab in _plainPrefabs)
        {
            if (prefab == null) continue;
            var instance = Instantiate(prefab);
            if (_persistAcrossScenes)
                DontDestroyOnLoad(instance);
        }

        foreach (var prefab in _servicePrefabs)
        {
            if (prefab == null) continue;

            var instance = Instantiate(prefab);

            if (_persistAcrossScenes)
                DontDestroyOnLoad(instance.gameObject);

            foreach (var component in instance.GetComponents<MonoBehaviour>())
            {
                if (component is not IService service) continue;

                var concreteType = component.GetType();
                typeof(Services).GetMethod("Register")
                    .MakeGenericMethod(concreteType)
                    .Invoke(null, new object[] { component });
                _registeredServiceTypes.Add(concreteType);
                _services.Add(service);
            }
        }

        await Task.WhenAll(_services.Select(s => s.InitializeAsync()));
    }

    public virtual void Dispose()
    {
        var unregisterMethod = typeof(Services).GetMethod("Unregister");

        foreach (var type in _registeredServiceTypes)
            unregisterMethod.MakeGenericMethod(type).Invoke(null, null);

        _registeredServiceTypes.Clear();
        _services.Clear();
    }

    private void OnDestroy()
    {
        Dispose();
    }
}

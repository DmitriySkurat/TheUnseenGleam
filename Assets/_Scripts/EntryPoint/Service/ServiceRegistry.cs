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
            if (prefab != null) Instantiate(prefab);

        foreach (var prefab in _servicePrefabs)
        {
            if (prefab == null) continue;

            var instance = Instantiate(prefab);

            if (_persistAcrossScenes)
                DontDestroyOnLoad(instance.gameObject);

            if (instance is IService service)
            {
                Services.Register(instance);
                _registeredServiceTypes.Add(instance.GetType());
                _services.Add(service);
            }
        }

        await Task.WhenAll(_services.Select(s => s.InitializeAsync()));
    }

    public virtual void Dispose()
    {
        foreach (var type in _registeredServiceTypes)
            Services.Unregister(type);

        _registeredServiceTypes.Clear();
        _services.Clear();
    }

    private void OnDestroy()
    {
        Dispose();
    }
}

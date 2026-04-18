using System;
using System.Collections.Generic;
using UnityEngine;

public static class Services
{
    private static readonly Dictionary<Type, object> _services = new();


    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);

        if (_services.ContainsKey(type))
            throw new Exception($"Service {type.Name} already registered");

        _services[type] = service;
    }

    public static void Unregister<T>() where T : class
    {
        var type = typeof(T);

        if (_services.ContainsKey(type))
            _services.Remove(type);
    }

    public static bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var service))
            return service as T;

        throw new Exception($"Service {type.Name} not found");
    }

    // public static void Update()
    // {
    //     foreach (var type in _services.Keys)
    //         Debug.Log(type.Name);
    // }

    public static void Clear()
    {
        _services.Clear();
    }
}
using System;
using System.Collections.Generic;

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

    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var service))
            return service as T;

        throw new Exception($"Service {type.Name} not found");
    }

    public static void Clear()
    {
        _services.Clear();
    }
}
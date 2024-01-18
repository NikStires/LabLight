using System;
using System.Collections.Generic;
/// <summary>
/// Static class for accessing swappable services
/// -logger (or chain of loggers)
/// -Dataprovider
/// </summary>
public class ServiceRegistry
{
    /// </summary>
    private static Dictionary<Type, object> registry =  new Dictionary<Type, object>();

    public static void RegisterService<T>(T service)
    {
        Type interfaceType = typeof(T);
        registry[interfaceType] = service;
    }

    public static void UnRegisterService<T>()
    {
        Type interfaceType = typeof(T);
        registry.Remove(interfaceType);
    }

    public static T GetService<T>()
    {
        object service;

        Type interfaceType = typeof(T);
        if (registry.TryGetValue(interfaceType, out service))
        {
            return (T)service;
        }

        return default(T);
    }

    /// <summary>
    /// Cached logger lookup
    /// </summary>
    private static LoggerImpl _logger;
    public static LoggerImpl Logger
    {
        get
        {
            if (_logger == null)
            {
                _logger = GetService<LoggerImpl>();
            }            
            return _logger;
        }
    }
}
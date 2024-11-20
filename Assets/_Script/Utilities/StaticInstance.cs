using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A static instance is similar to a singleton, but instead of destroying any new
/// instances, it overrides the current instance. This is handy for resetting the state
/// and saves you doing it manually
/// </summary>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour {
    public static T Instance { get; private set; }
    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit() {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// This transforms the static instance into a basic singleton. This will destroy any new
/// versions created, leaving the original instance intact
/// </summary>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour {
    protected override void Awake() {
        if (Instance != null) Destroy(gameObject);
        base.Awake();
    }
}

/// <summary>
/// Finally we have a persistent version of the singleton. This will survive through scene
/// loads. Perfect for system classes which require stateful, persistent data. Or audio sources
/// where music plays through loading screens, etc
/// </summary>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour {
    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}

[DefaultExecutionOrder(-100)]
public abstract class ServiceLocatorSingleton<T, S> : Singleton<T> where T : MonoBehaviour
{
    private readonly Dictionary<string, List<S>> services = new ();
    public List<T> Get<T>() where T : S
    {
        string key = typeof(T).Name;
        if (!services.ContainsKey(key))
        {
            throw new InvalidOperationException($"No services registered for type {key}.");
        }

        // Attempt to cast each service to the desired type and return the list
        return services[key].OfType<T>().ToList();
    }
    public void Register<T>(T service) where T : S
    {
        string key = typeof(T).Name;
        if (!this.services.ContainsKey(key))
        {
            this.services[key] = new List<S>();
        }

        this.services[key].Add(service);
    }
    
    public void Unregister<T>(T service) where T : S
    {
        string key = typeof(T).Name;
        if (!this.services.ContainsKey(key))
        {
            return;
        }

        this.services[key].Remove(service);
        if (this.services[key].Count == 0)
        {
            this.services.Remove(key);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Script.Utilities.ServiceLocator
{
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : Singleton<ServiceLocator>
    {
        private readonly Dictionary<string, List<IGameService>> _services = new ();
        public List<T> Get<T>() where T : IGameService
        {
            
            string key = typeof(T).Name;
            if (!_services.TryGetValue(key, out var service))
            {
                Debug.LogWarning($"Service of type {key} not found");
            }

            // Attempt to cast each service to the desired type and return the list
            return service != null ? service.OfType<T>().ToList() : new List<T>();
        }
        public void Register<T>(T service) where T : IGameService
        {
            Debug.Log("Registering service " + service);
            string key = typeof(T).Name;
            if (!this._services.ContainsKey(key))
            {
                this._services[key] = new List<IGameService>();
            }

            this._services[key].Add(service);
        }
    
        public void Unregister<T>(T service) where T : IGameService
        {
            string key = typeof(T).Name;
            if (!this._services.ContainsKey(key))
            {
                return;
            }

            this._services[key].Remove(service);
            if (this._services[key].Count == 0)
            {
                this._services.Remove(key);
            }
        }
    }
}
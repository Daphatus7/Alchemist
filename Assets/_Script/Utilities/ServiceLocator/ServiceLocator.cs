using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Script.Utilities.ServiceLocator
{
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : Singleton<ServiceLocator>
    {
        private readonly Dictionary<string, List<IGameService>> services = new ();
        public List<T> Get<T>() where T : IGameService
        {
            
            string key = typeof(T).Name;
            if (!services.TryGetValue(key, out var service))
            {
                throw new InvalidOperationException($"No services registered for type {key}.");
            }

            // Attempt to cast each service to the desired type and return the list
            return service.OfType<T>().ToList();
        }
        public void Register<T>(T service) where T : IGameService
        {
            Debug.Log("Registering service " + service);
            string key = typeof(T).Name;
            if (!this.services.ContainsKey(key))
            {
                this.services[key] = new List<IGameService>();
            }

            this.services[key].Add(service);
        }
    
        public void Unregister<T>(T service) where T : IGameService
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
}
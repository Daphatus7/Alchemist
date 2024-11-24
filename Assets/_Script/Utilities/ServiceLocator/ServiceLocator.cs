using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Script.Utilities.ServiceLocator
{
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : ServiceLocatorSingleton<ServiceLocator, IGameService> 
    {
        private readonly Dictionary<string, List<IGameService>> services = new ();
        public new List<T> Get<T>() where T : IGameService
        {
            string key = typeof(T).Name;
            if (!services.ContainsKey(key))
            {
                throw new InvalidOperationException($"No services registered for type {key}.");
            }

            // Attempt to cast each service to the desired type and return the list
            return services[key].OfType<T>().ToList();
        }
        public new void Register<T>(T service) where T : IGameService
        {
            string key = typeof(T).Name;
            if (!this.services.ContainsKey(key))
            {
                this.services[key] = new List<IGameService>();
            }

            this.services[key].Add(service);
        }
    
        public new void Unregister<T>(T service) where T : IGameService
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
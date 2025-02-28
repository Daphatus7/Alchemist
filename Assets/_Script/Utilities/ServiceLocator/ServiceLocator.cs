using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Utilities.ServiceLocator
{
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : Singleton<ServiceLocator>
    {
        // Dictionary to hold services with their type names as keys
        private readonly Dictionary<string, IGameService> _services = new();

        /// <summary>
        /// Get a service of type T.
        /// </summary>
        public T Get<T>() where T : IGameService
        {
            string key = typeof(T).Name;
            if (_services.TryGetValue(key, out var service))
            {
                return (T)service;
            }

            Debug.LogWarning($"Service of type {key} not found");
            return default; // Returns null for reference types
        }

        /// <summary>
        /// Register a service of type T.
        /// </summary>
        public void Register<T>(T service) where T : IGameService
        {
            //Debug.Log($"Registering service of type {typeof(T).Name}");
            string key = typeof(T).Name;

            if (_services.TryAdd(key, service)) return;
            Debug.LogError($"Service of type {key} is already registered.");
        }

        /// <summary>
        /// Unregister a service of type T.
        /// </summary>
        public void Unregister<T>() where T : IGameService
        {
            string key = typeof(T).Name;

            if (_services.Remove(key))
            {
                Debug.Log($"Service of type {key} unregistered successfully.");
            }
            else
            {
                Debug.LogWarning($"No service of type {key} found to unregister.");
            }
        }
    }
}
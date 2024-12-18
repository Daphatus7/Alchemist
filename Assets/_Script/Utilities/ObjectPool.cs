// Author : Peiyu Wang @ Daphatus
// 18 12 2024 12 42

using System.Collections.Generic;
using UnityEngine;

namespace _Script.Utilities
{
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public ObjectPool(T prefab, Transform parent = null, int initialCapacity = 10)
        {
            _prefab = prefab;
            _parent = parent;

            // Pre-warm the pool
            for (int i = 0; i < initialCapacity; i++)
            {
                var obj = Object.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = Object.Instantiate(_prefab, _parent);
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        public void ReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
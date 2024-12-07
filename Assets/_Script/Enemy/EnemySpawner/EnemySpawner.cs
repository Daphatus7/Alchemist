// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 06

using System;
using UnityEngine;

namespace _Script.Enemy.EnemySpawner
{
    [RequireComponent(typeof(Transform))]
    public class EnemySpawner : MonoBehaviour
    {
        private Transform _transform;
        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }
        
        public Vector2 GetSpawnPosition()
        {
            return _transform.position;
        }
    }
}
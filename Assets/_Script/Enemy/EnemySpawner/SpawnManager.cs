// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 43

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.Enemy.EnemySpawner
{
    public class SpawnManager : Singleton<SpawnManager>
    {
        private List<EnemySpawner> _spawners;
        [SerializeField] private int maxSpawners = 10;
        [SerializeField] private GameObject enemyPrefab;
        private void Awake()
        {
            _spawners = new List<EnemySpawner>();
            var spawners = FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                _spawners.Add(spawner);
            }
        }

        public void Start()
        {
            //spawn enemies at spawn points
            for (int i = 0; i < maxSpawners; i++)
            {
                var spawnPosition = GetRandomSpawnPosition(); 
                SpawnEnemy(spawnPosition);
            }
        }

        private void SpawnEnemy(Vector2 spawnPosition)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        private Vector2 GetRandomSpawnPosition()
        {
            if (_spawners.Count == 0)
            {
                Debug.LogWarning("No spawners found");
                return Vector2.zero;
            }
            var randomIndex = Random.Range(0, _spawners.Count);
            return _spawners[randomIndex].GetSpawnPosition();
        }
    }
}
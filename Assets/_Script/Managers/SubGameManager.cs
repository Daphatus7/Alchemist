// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 38

using System;
using _Script.Map.Procedural;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Managers
{
    public class SubGameManager: Singleton<SubGameManager>
    {
        [SerializeField] private ProceduralMapGenerator _map;
        [SerializeField] private Vector2Int _spawnPoint;
        [SerializeField] private Vector2Int _endPoint;
        [SerializeField] private Vector2Int _mapSize;
        [SerializeField] private int _minMapSize = 10;
        [SerializeField] private int _maxMapSize = 100;


        private Vector2Int GenerateMapSize()
        {
            return new Vector2Int(UnityEngine.Random.Range(_minMapSize, _maxMapSize), UnityEngine.Random.Range(_minMapSize, _maxMapSize));
        }
        
        public bool GenerateMap(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            _mapSize = GenerateMapSize();
            return _map.GenerateMap(_mapSize.x, _mapSize.y,  out spawnPoint, out endPoint);
        }
        
        [Button("Generate Map")]
        private void EditorGenerateMap()
        {
            Vector2Int s, e;
            if (GenerateMap(out s, out e))
            {
                _spawnPoint = s;
                _endPoint = e;
                Debug.Log($"Map generated with spawn at {_spawnPoint} and end at {_endPoint}.");
            }
            else
            {
                Debug.LogWarning("Failed to generate map.");
            }
        }
    }
}
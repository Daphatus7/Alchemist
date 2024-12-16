// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 38

using System;
using _Script.Map.Procedural;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Managers
{

    public enum SubGameType
    {
        ResourceGathering, // Harvesting, Mining, Lumbering, etc.
        Dungeon, // Dungeon crawling with enemies and loot
        BossFight, // Boss fight with unique mechanics
        Bonfire, // Resting and upgrading
    }
    
    public class SubGameManager: Singleton<SubGameManager>
    {
        [SerializeField] private ProceduralMapGenerator _map;
        [SerializeField] private Vector2Int _spawnPoint;
        [SerializeField] private Vector2Int _endPoint;
        [SerializeField] private Vector2Int _mapSize; public Vector2Int MapSize => _mapSize;
        [SerializeField] private int _minMapSize = 10;
        [SerializeField] private int _maxMapSize = 100;
        [SerializeField] private SubGameType _subGameType = SubGameType.ResourceGathering;
        
        
        public bool GenerateMap(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            // Now that the pathfinder is set up, run your map generation based on _subGameType
            switch (_subGameType)
            {
                case SubGameType.ResourceGathering:
                    return ResourceGathering_MapGeneration(out spawnPoint, out endPoint);
                case SubGameType.Dungeon:
                    return Dungeon_MapGeneration(out spawnPoint, out endPoint);
                case SubGameType.BossFight:
                    return BossFight_MapGeneration(out spawnPoint, out endPoint);
                case SubGameType.Bonfire:
                    return BonfireMap_Generation(out spawnPoint, out endPoint);
                default:
                    return BonfireMap_Generation(out spawnPoint, out endPoint);
            }
        }
        
        private bool ResourceGathering_MapGeneration(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            _mapSize = GenerateMapSize();
            return _map.GenerateMap(_mapSize.x, _mapSize.y,  out spawnPoint, out endPoint);
        }
        
        private bool Dungeon_MapGeneration(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            _mapSize = GenerateMapSize();
            return _map.GenerateMap(_mapSize.x, _mapSize.y,  out spawnPoint, out endPoint);
        }
        
        private bool BossFight_MapGeneration(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            _mapSize = GenerateMapSize();
            return _map.GenerateMap(_mapSize.x, _mapSize.y,  out spawnPoint, out endPoint);
        }
        
        private bool BonfireMap_Generation(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            _mapSize = GenerateMapSize();
            return _map.GenerateMap(_mapSize.x, _mapSize.y,  out spawnPoint, out endPoint);
        }
        
        private Vector2Int GenerateMapSize()
        {
            return new Vector2Int(UnityEngine.Random.Range(_minMapSize, _maxMapSize), UnityEngine.Random.Range(_minMapSize, _maxMapSize));
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
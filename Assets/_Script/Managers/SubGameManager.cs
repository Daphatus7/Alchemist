// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 38

using System;
using _Script.Map.Generators;
using _Script.Map.Procedural;
using _Script.Map.WorldMap;
using _Script.Map.WorldMap.MapNode;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Managers
{
    
    public class SubGameManager: Singleton<SubGameManager>
    {
        [SerializeField] private ProceduralMapGenerator _map;
        [SerializeField] private Vector2Int _spawnPoint;
        [SerializeField] private Vector2Int _endPoint;
        [SerializeField] private Vector2Int _mapSize; public Vector2Int MapSize => _mapSize;
        [SerializeField] private int _minMapSize = 10;
        [SerializeField] private int _maxMapSize = 100;
        [ReadOnly] private NodeType _subGameType;
        
        public bool GenerateMap(NodeData nodeData,out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            _subGameType = nodeData.NodeType;
            // Now that the pathfinder is set up, run your map generation based on _subGameType
            switch (_subGameType)
            {
                case NodeType.Resource:
                    return ResourceGathering_MapGeneration(out spawnPoint, out endPoint);
                case NodeType.Enemy:
                    return Dungeon_MapGeneration(out spawnPoint, out endPoint);
                case NodeType.Boss:
                    return BossFight_MapGeneration(out spawnPoint, out endPoint);
                case NodeType.Bonfire:
                    return BonfireMap_MapGeneration(out spawnPoint, out endPoint);
                default:
                    throw new ArgumentOutOfRangeException();
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
            spawnPoint = new Vector2Int(0, 0);
            endPoint = spawnPoint;
            return true;
        }
        
        private bool BonfireMap_MapGeneration(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            spawnPoint = new Vector2Int(0, 0);
            endPoint = spawnPoint;
            return true;
        }
        
        private Vector2Int GenerateMapSize()
        {
            return new Vector2Int(UnityEngine.Random.Range(_minMapSize, _maxMapSize), UnityEngine.Random.Range(_minMapSize, _maxMapSize));
        }
        
        // [Button("Generate Map")]
        // private void EditorGenerateMap()
        // {
        //     Vector2Int s, e;
        //     if (GenerateMap(GenerateNodeData(), out s, out e))
        //     {
        //         _spawnPoint = s;
        //         _endPoint = e;
        //         Debug.Log($"Map generated with spawn at {_spawnPoint} and end at {_endPoint}.");
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Failed to generate map.");
        //     }
        // }
        //
        // private static NodeData GenerateNodeData(NodeType nodeType, string description, int seed)
        // {
        //     switch (nodeType)
        //     {
        //         case NodeType.Resource:
        //             return new ResourceMapNode(description, seed);
        //         case NodeType.Enemy:
        //             return new EnemyMapNode(description, seed);
        //         case NodeType.Boss:
        //             return new BossNode(description, seed, "Boss");
        //         case NodeType.Bonfire:
        //             return new BonfireMapNode(description, seed);
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }
    }
}
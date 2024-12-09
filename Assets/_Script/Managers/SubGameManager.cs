// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 38

using System;
using _Script.Map.Procedural;
using UnityEngine;

namespace _Script.Managers
{
    public class SubGameManager: Singleton<SubGameManager>
    {
        [SerializeField] private ProceduralMapGenerator _map;
        [SerializeField] private Vector2Int _spawnPoint;
        [SerializeField] private Vector2Int _endPoint;
        
        public bool GenerateMap(out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            return _map.GenerateMap(out spawnPoint, out endPoint);
        }
        
        
        
    }
}
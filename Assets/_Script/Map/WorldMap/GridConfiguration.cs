using System;
using System.Collections.Generic;
using System.Linq;
using _Script.Character.PlayerRank;
using UnityEngine;

namespace _Script.Map.WorldMap
{
    public class GridConfiguration
    {
        public int GridRadius;
        // Instead of a dictionary, store a list of KeyValuePairs or a custom struct

        // Precomputed cumulative weights
        private readonly List<(NodeType type, int cumulativeWeight)> _cumulativeList;
        private readonly int _totalWeight;
        
        public GridConfiguration(
            int pathDifficulty = 1,
            Vector3Int startPosition = default,//this should be adjusted to hexgrid position
            Vector3Int endPosition = default,
            //Weight
            int gridRadius = 10,
            int resourceWeight = 8,
            int enemyWeight = 5,
            int campfireWeight = 1,
            int bossWeight = 1)
        {
            GridRadius = gridRadius;

            // Initialize the base weights
            List<(NodeType type, int weight)> weightedTypes = new()
            {
                (NodeType.Resource, resourceWeight),
                (NodeType.Enemy, enemyWeight),
                (NodeType.Bonfire, campfireWeight),
                (NodeType.Boss, bossWeight),
            };

            // Precompute cumulative weights
            _cumulativeList = new List<(NodeType, int)>(weightedTypes.Count);
            int cumulative = 0;
            foreach (var (type, w) in weightedTypes)
            {
                cumulative += w;
                _cumulativeList.Add((type, cumulative));
            }
            _totalWeight = cumulative;
        }

        public NodeType GetRandomType()
        {
            int rand = UnityEngine.Random.Range(0, _totalWeight);

            // Since _cumulativeList is sorted by cumulativeWeight, we can do a binary search.
            // For simplicity, we just do a linear pass here, but binary search is possible.
            foreach (var (type, cWeight) in _cumulativeList)
            {
                if (rand < cWeight)
                {
                    return type;
                }
            }

            // Fallback, should never reach here if totalWeight > 0
            return NodeType.Boss;
        }
    }
}
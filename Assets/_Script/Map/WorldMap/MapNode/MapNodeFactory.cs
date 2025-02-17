// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 45

using _Script.Character.PlayerRank;
using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    public class MapNodeFactory : Singleton<MapNodeFactory>
    {
        [SerializeField] private BossMapNode[] _bossNodes;
        [SerializeField] private EnemyMapNode[] _enemyNodes;
        [SerializeField] private ResourceMapNode[] _resourceNodes;
        [SerializeField] private BonfireMapNode[] _bonfireNodes;

        /// <summary>
        /// Creates a new NodeDataInstance based on the specified node type, rank, and seed.
        /// The provided seed is used to initialize the random generator for deterministic behavior.
        /// </summary>
        public NodeDataInstance CreateNode(NodeType nodeType, PlayerRankEnum mapRank, int seed)
        {
            // Seed the random generator for deterministic selection.
            Random.InitState(seed);

            return nodeType switch
            {
                NodeType.Boss     => CreateNodeFromArray(_bossNodes, NodeType.Boss, mapRank),
                NodeType.Enemy    => CreateNodeFromArray(_enemyNodes, NodeType.Enemy, mapRank),
                NodeType.Resource => CreateNodeFromArray(_resourceNodes, NodeType.Resource, mapRank),
                NodeType.Bonfire  => CreateNodeFromArray(_bonfireNodes, NodeType.Bonfire, mapRank),
                _                 => null,
            };
        }

        /// <summary>
        /// Selects a random node from the provided array and creates a NodeDataInstance from it.
        /// Returns null if the array is null or empty.
        /// </summary>
        private NodeDataInstance CreateNodeFromArray<T>(T[] nodes, NodeType nodeType, PlayerRankEnum mapRank) where T : NodeData
        {
            if (nodes == null || nodes.Length == 0)
            {
                Debug.LogWarning($"No nodes available for node type: {nodeType}");
                return null;
            }

            // Select a random index once to get a consistent node.
            int index = Random.Range(0, nodes.Length);
            T selectedNode = nodes[index];

            if (selectedNode == null)
            {
                Debug.LogWarning($"Selected node at index {index} is null for node type: {nodeType}");
                return null;
            }

            return new NodeDataInstance(selectedNode.MapName, selectedNode.Description, nodeType, mapRank);
        }
    }
}
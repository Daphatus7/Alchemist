// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 45

using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    public class MapNodeFactory : Singleton<MapNodeFactory>
    {
        [SerializeField] private BossMapNode [] _bossNodes;
        [SerializeField] private EnemyMapNode [] _enemyNodes;
        [SerializeField] private ResourceMapNode [] _resourceNodes;
        [SerializeField] private BonfireMapNode [] _bonfireNodes;


        public NodeData CreateNode(NodeType nodeType, string description, int seed)
        {
            return nodeType switch
            {
                NodeType.Boss => _bossNodes[Random.Range(0, _bossNodes.Length)],
                NodeType.Enemy => _enemyNodes[Random.Range(0, _enemyNodes.Length)],
                NodeType.Resource => _resourceNodes[Random.Range(0, _resourceNodes.Length)],
                NodeType.Bonfire => _bonfireNodes[Random.Range(0, _bonfireNodes.Length)],
                _ => _bonfireNodes[Random.Range(0, _bonfireNodes.Length)]
            };
        }
    }
}
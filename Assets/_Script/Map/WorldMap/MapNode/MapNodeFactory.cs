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
            switch (nodeType)
            {
                case NodeType.Boss:
                    return _bossNodes[Random.Range(0, _bossNodes.Length)];
                case NodeType.Enemy:
                    return _enemyNodes[Random.Range(0, _enemyNodes.Length)];
                case NodeType.Resource:
                    return _resourceNodes[Random.Range(0, _resourceNodes.Length)];
                case NodeType.Bonfire:
                    return _bonfireNodes[Random.Range(0, _bonfireNodes.Length)];
                default:
                    return _bonfireNodes[Random.Range(0, _bonfireNodes.Length)];
            }
        }
    }
}
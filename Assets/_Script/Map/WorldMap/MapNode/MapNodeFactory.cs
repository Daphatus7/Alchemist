// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 45

using _Script.Character.PlayerRank;
using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    public class MapNodeFactory : Singleton<MapNodeFactory>
    {
        [SerializeField] private BossMapNode [] _bossNodes;
        [SerializeField] private EnemyMapNode [] _enemyNodes;
        [SerializeField] private ResourceMapNode [] _resourceNodes;
        [SerializeField] private BonfireMapNode [] _bonfireNodes;


        public NodeDataInstance CreateNode(NodeType nodeType, PlayerRankEnum mapRank, int seed)
        {
            return nodeType switch
            {
                NodeType.Boss => new NodeDataInstance(_bossNodes[Random.Range(0, _bossNodes.Length)], mapRank),
                NodeType.Enemy => new NodeDataInstance(_enemyNodes[Random.Range(0, _enemyNodes.Length)], mapRank),
                NodeType.Resource => new NodeDataInstance(_resourceNodes[Random.Range(0, _resourceNodes.Length)], mapRank),
                NodeType.Bonfire => new NodeDataInstance(_bonfireNodes[Random.Range(0, _bonfireNodes.Length)], mapRank),
                _ => new NodeDataInstance(_bonfireNodes[Random.Range(0, _bonfireNodes.Length)], mapRank)
            };
        }
    }
}
// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 56

using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    [CreateAssetMenu(fileName = "EnemyMapNode", menuName = "MapNode/EnemyMapNode")]
    public class EnemyMapNode : NodeData
    {
        public override NodeType NodeType => NodeType.Enemy;
        
        public string [] enemyNames;
        
        public float enemySpawnRate;
    }
}
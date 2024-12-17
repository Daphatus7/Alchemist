// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 55

using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    [CreateAssetMenu(fileName = "BossMapNode", menuName = "MapNode/BossMapNode")]
    public class BossMapNode : NodeData
    {
        public string BossName;
        public override NodeType NodeType => NodeType.Boss;

        public BossMapNode(string description, int seed, string bossName) : base(description, seed)
        {
            BossName = bossName;
        }

    }
}
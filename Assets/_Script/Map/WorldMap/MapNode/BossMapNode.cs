// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 55

using _Script.Managers;
using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    //Mark as obsolete
    [System.Obsolete("Deprecated")]
    [CreateAssetMenu(fileName = "BossMapNode", menuName = "MapNode/BossMapNode")]
    public class BossMapNode : NodeData
    {
        public string BossName;
        public string SceneName;
        public override NodeType NodeType => NodeType.Boss;
    }
}
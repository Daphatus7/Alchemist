// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 27

using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    [CreateAssetMenu(fileName = "BonfireMapNode", menuName = "MapNode/BonfireMapNode")]
    public class BonfireMapNode : NodeData
    {
        public override NodeType NodeType => NodeType.Bonfire;
    }
}
// Author : Peiyu Wang @ Daphatus
// 17 12 2024 12 32

using UnityEngine;

namespace _Script.Map.WorldMap.MapNode
{
    [CreateAssetMenu(fileName = "ResourceMapNode", menuName = "MapNode/ResourceMapNode")]
    public class ResourceMapNode : NodeData
    {
        public override NodeType NodeType => NodeType.Resource;
        
        public string [] resourceNames;
        
        public float resourceSpawnRate;
        
        public ResourceMapNode(string description, int seed) : base(description, seed)
        {
            
        }
    }
}
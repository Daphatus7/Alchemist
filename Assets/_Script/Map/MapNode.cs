// Author : Peiyu Wang @ Daphatus
// 08 12 2024 12 48

using _Script.Map.Hexagon_Graph;
using UnityEngine;

namespace _Script.Map
{
    public class MapNode
    {
        public NodeType NodeType = NodeType.Boss;
        public readonly string MapName = "AlphaDungeon2";
        public string Description = "This is a boss node";
        public int Seed = 0;
        
        public MapNode(NodeType nodeType, string nodeName, string description, int seed)
        {
            NodeType = nodeType;
            MapName = nodeName;
            Description = description;
            Seed = seed;
        }
        public MapNode()
        {
        }
    }
}
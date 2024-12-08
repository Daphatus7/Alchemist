// Author : Peiyu Wang @ Daphatus
// 08 12 2024 12 48

using _Script.Map.Hexagon_Graph;
using UnityEngine;

namespace _Script.Map
{
    public class MapNode :ScriptableObject
    {
        public NodeType nodeType = NodeType.Boss;
        public string description = "This is a boss node";
        public int seed = 0;
    }
}
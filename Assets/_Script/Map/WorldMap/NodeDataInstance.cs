// Author : Peiyu Wang @ Daphatus
// 14 02 2025 02 07

using _Script.Character.PlayerRank;
using _Script.Map.WorldMap.MapNode;

namespace _Script.Map.WorldMap
{
    public class NodeDataInstance
    {
        public NodeData NodeData { get; protected set; }
        public PlayerRankEnum MapRank { get; protected set; }
        
        //Insert generation instruction
        
        public NodeDataInstance(NodeData nodeData, PlayerRankEnum mapRank)
        {
            NodeData = nodeData;
            MapRank = mapRank;
        }
    }
}
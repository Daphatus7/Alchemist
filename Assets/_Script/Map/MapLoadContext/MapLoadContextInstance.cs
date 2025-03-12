// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 05

using _Script.Character.PlayerRank;

namespace _Script.Map.MapLoadContext
{
    public abstract class MapLoadContextInstance
    {
        public NiRank MapRank { get; set; }
        public string MapName { get; set; }
    }
}
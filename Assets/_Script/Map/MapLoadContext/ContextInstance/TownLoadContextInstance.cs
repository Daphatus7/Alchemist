// Author : Peiyu Wang @ Daphatus
// 24 03 2025 03 00

using _Script.Character.PlayerRank;

namespace _Script.Map.MapLoadContext.ContextInstance
{
    public class TownLoadContextInstance : MapLoadContextInstance
    {
        public TownLoadContextInstance(NiRank mapRank, string mapName, RewardContext.RewardContext rewardContext) 
            : base(mapRank, mapName, rewardContext)
        {
        }
    }
}
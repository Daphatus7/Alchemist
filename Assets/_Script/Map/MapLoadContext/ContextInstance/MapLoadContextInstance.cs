// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 05

using _Script.Character.PlayerRank;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Map.MapLoadContext.ContextInstance
{
    public abstract class MapLoadContextInstance
    {
        private NiRank _mapRank; public NiRank MapRank => _mapRank;
        
        /// <summary>
        /// Scene to load
        /// </summary>
        private string _mapName; public string MapName => _mapName;

        /// <summary>
        /// usually 2-3 items
        /// </summary>
        private ItemData[] _rewardItems; public ItemData[] RewardItems => _rewardItems;
        
        protected MapLoadContextInstance(NiRank mapRank, string mapName, RewardContext.RewardContext reward)
        {
            _mapRank = mapRank;
            _mapName = mapName;
            _rewardItems = reward.ItemRewards;
        }
    }
}
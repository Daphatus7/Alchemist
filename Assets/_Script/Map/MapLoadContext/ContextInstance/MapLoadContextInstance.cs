// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 05

using _Script.Character.PlayerRank;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Map.MapLoadContext.RewardContext;
using UnityEngine;

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
        /// whethere
        /// </summary>
        private bool _isCompleted; public bool IsCompleted
        {
            get => _isCompleted;
            set => _isCompleted = value;
        }

        /// <summary>
        /// usually 2-3 items
        /// </summary>
        private ItemData[] _rewardItems; public ItemData[] RewardItems => _rewardItems;
        public RewardType RewardType;
        
        protected MapLoadContextInstance(NiRank mapRank, string mapName, RewardContext.RewardContext reward)
        {
            _mapRank = mapRank;
            _mapName = mapName;
            _rewardItems = reward.ItemRewards;
        }
    }
}
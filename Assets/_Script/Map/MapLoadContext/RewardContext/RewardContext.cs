// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 57

using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Map.MapLoadContext.RewardContext
{
    
    /// <summary>
    /// Contains the reward of the same type
    /// </summary>
    public class RewardContext
    {
        
        public RewardType RewardType { get; }
        public ItemData[] ItemRewards { get; private set; }
        public RewardContext(ItemData[] itemRewards, RewardType rewardType)
        {
            RewardType = rewardType;
            ItemRewards = itemRewards;
        }
    }
    
    public enum RewardType
    {
        Equipment,
        Supply,
        Boss,
    }
}
// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 57

using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Map.MapLoadContext.RewardContext
{
    public class RewardContext
    {
        public RewardContext(ItemData[] itemRewards)
        {
            ItemRewards = itemRewards;
        }

        public ItemData[] ItemRewards { get; }
    }
}
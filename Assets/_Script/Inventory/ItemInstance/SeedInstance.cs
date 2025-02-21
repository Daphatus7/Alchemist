// Author : Peiyu Wang @ Daphatus
// 21 02 2025 02 45

using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class SeedInstance : ItemInstance
    {
        public SeedInstance(ItemData itemData, bool rotated, int quantity = 1) : base(itemData, rotated, quantity)
        {
        }

        public Sprite SeedOnGroundSprite => ((SeedItem) ItemData).seedOnGroundSprite;
    }
}
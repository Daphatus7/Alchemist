// Author : Peiyu Wang @ Daphatus
// 21 02 2025 02 57

using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class ConsumableItemInstance : ItemInstance
    {
        public ConsumableItemInstance(ItemData itemData, bool rotated, int quantity = 1) : base(itemData, rotated, quantity)
        {
        }

        public override bool Use(PlayerCharacter player)
        {
            Debug.Log(ItemData.ItemName + " is used.");
            return ItemData.Use(player);
        }
    }
}
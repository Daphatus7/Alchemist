// Author : Peiyu Wang @ Daphatus
// 21 02 2025 02 37

using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Inventory.ItemInstance
{
    public class EquipmentInstance : ItemInstance
    {
        public EquipmentInstance(ItemData itemData, bool rotated, int quantity = 1) : base(itemData, rotated, quantity)
        {
        }
    }
}
using _Script.Items.AbstractItemTypes;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Items
{
    public static class ItemConversion
    {
        /**
         * Convert the item data to equipment item
         */
        public static EquipmentItem ConvertToEquipmentItem(ItemData itemData)
        {
            if (itemData is EquipmentItem equipmentItem)
            {
                return equipmentItem;
            }
            return null;
        }

        /**
         * Convert EquipmentItem to WeaponItem
         */
        public static WeaponItem ConvertToWeaponItem(EquipmentItem equipmentItem)
        {
            if (equipmentItem is WeaponItem weaponItem)
            {
                return weaponItem;
            }

            return null;
        }

        /**
         * Convert EquipmentItem to Tool
         */
        public static Tool ConvertToTool(EquipmentItem equipmentItem)
        {
            if (equipmentItem is Tool tool)
            {
                return tool;
            }

            return null;
        }
        
        /*
         * Convert the item data to consumable
         */
        public static ConsumableItem ConvertToConsumable(ItemData itemData)
        {
            if (itemData is ConsumableItem consumableItem)
            {
                return consumableItem;
            }

            return null;
        }
        
        public static SeedItem ConvertToSeed(ItemData itemData)
        {
            if (itemData is SeedItem seedItem)
            {
                return seedItem;
            }

            return null;
        }
    }
}
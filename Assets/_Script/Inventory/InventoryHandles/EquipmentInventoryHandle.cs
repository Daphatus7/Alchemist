using _Script.Inventory.InventoryBackend;
using _Script.Items;

namespace _Script.Inventory.EquipmentBackend
{
    /**
     * handle for equipment inventory
     */
    public interface IPlayerEquipmentHandle
    {
        /**
         * Apply the item effect to the player
         */
        InventoryItem Handle_Equip(EquipmentItem equipmentItem);
        
        /**
         * Remove the item effect from the player
         */
        InventoryItem Handle_Unequip_RemoveEffect(InventorySlot fromSlo);
    }
}
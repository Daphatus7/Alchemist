using _Script.Inventory.InventoryBackend;
using _Script.Items;

namespace _Script.Inventory.EquipmentBackend
{
    /**
     * handle for equipment inventory
     */
    public interface IPlayerEquipmentHandle
    {
        InventoryItem Handle_EquipItem(InventoryItem item);
        InventoryItem Handle_UnequipItem(InventorySlot fromSlo);
    }
}
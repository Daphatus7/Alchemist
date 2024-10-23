using _Script.Inventory.InventoryHandles;
using _Script.Items;

namespace _Script.Inventory.InventoryBackend
{
    public class PlayerEquipmentInventory : Inventory, IEquipmentInventoryHandle
    {
        public bool Handle_AddItem(ItemData itemData, int quantity)
        {
            return AddItem(itemData, quantity);
        }
        
        public bool Handle_RemoveItem(ItemData itemData, int quantity)
        {
            return RemoveItem(itemData, quantity);
        }
    }
}
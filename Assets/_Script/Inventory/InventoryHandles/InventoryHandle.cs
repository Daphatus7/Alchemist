using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Inventory.InventoryHandles
{
    /**
     * DO NOT USE THIS INTERFACE DIRECTLY
     * use the child interfaces instead
     */
    public interface IIventoryHandle
    {
        public bool Handle_AddItem(InventoryItem inventoryItem);
        public bool Handle_RemoveItem(InventoryItem inventoryItem);
        
    }
}
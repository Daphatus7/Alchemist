
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.SlotFrontend;
using _Script.Items;

namespace _Script.Inventory.ActionBarBackend
{
    public class ActionBar : InventoryBackend.Inventory, IActionBarHandle
    {
        private InventoryItem _selectedItem;
        
        public bool Handle_AddItem(InventoryItem inventoryItem)
        {
            return AddItem(inventoryItem);
        }

        public bool Handle_RemoveItem(InventoryItem inventoryItem)
        {
            return RemoveItem(inventoryItem);
        }
    }
}

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

        
        /// <summary>
        /// this provides interface for external classes to select the item
        /// No tangled logic
        /// Select the item in the slot and Call the method in the item
        /// </summary>
        /// <param name="slotIndex"></param>
        public void OnSelectItem(int slotIndex)
        {
            // {methods before} this function made sure that it's {not selecting the same item twice}
            //now select the new item and execute any action
            _selectedItem = Slots[slotIndex].Item;
            _selectedItem.ItemData.OnSelected(inventoryOwner);
        }
        
        /// <summary>
        /// handling selected none item
        /// </summary>
        public void OnSelectNone()
        {
            _selectedItem = null;
        }
        
        /// <summary>
        /// this provides interface for external classes to deselect the item
        /// Within the action bar class, there is {no} tangled logic
        /// DeSelect the item in the slot
        /// and call the method in the item
        /// </summary>
        /// <param name="slotIndex"></param>
        public void OnDeSelectItem(int slotIndex)
        {
            _selectedItem?.ItemData.OnDeselected(inventoryOwner);
            _selectedItem = null;
        }
        
    }
}
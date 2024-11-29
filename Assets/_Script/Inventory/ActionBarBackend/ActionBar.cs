
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.ActionBarBackend
{
    public class ActionBar : InventoryBackend.Inventory, IActionBarHandle
    {
        
        public bool Handle_AddItem(InventoryItem inventoryItem)
        {
            return AddItem(inventoryItem);
        }

        public bool Handle_RemoveItem(InventoryItem inventoryItem)
        {
            return RemoveItem(inventoryItem);
        }
        
        private InventoryItem _selectedItem;
        
        public void SetSelectedItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length)
            {
                _selectedSlotIndex = -1;
                _selectedItem = null;
            }
            else
            {
                Debug.Log("Selected item: " + slotIndex + " " + Slots[slotIndex].Item.ItemData.ItemName);
                _selectedSlotIndex = slotIndex;
                _selectedItem = Slots[slotIndex].Item;
            }
        }
        
        private int _selectedSlotIndex; public int SelectedSlotIndex => _selectedSlotIndex;
        
        /// <summary>
        /// this provides interface for external classes to select the item
        /// No tangled logic
        /// Select the item in the slot and Call the method in the item
        /// </summary>
        /// <param name="slotIndex"></param>
        public void OnSelectItem(int slotIndex)
        {
            SetSelectedItem(slotIndex);
            if (_selectedItem != null)
            {
                _selectedItem.ItemData.OnSelected(inventoryOwner);
            }
            else
            {
                Debug.LogWarning("Selected item is null.");
            }
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
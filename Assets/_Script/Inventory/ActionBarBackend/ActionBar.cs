
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
        
        private void SetSelectedItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length)
            {
                _selectedSlotIndex = -1;
                _selectedItem = null;
            }
            else
            {
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
                var itemType = _selectedItem.ItemData.ItemTypeString;
                if(itemType == "Seed")
                {
                    inventoryOwner.GenericStrategy.ChangeItem(_selectedItem.ItemData);
                    inventoryOwner.SetGenericStrategy();
                }
                else if (itemType == "Weapon")
                {
                    //spawn the weapon
                    //Let player handle the weapon
                    inventoryOwner.WeaponStrategy.ChangeWeapon(_selectedItem.ItemData);
                    //Set Strategy
                    inventoryOwner.SetWeaponStrategy();                }
                else
                {
                    Debug.LogWarning("Selected item is not a seed.");
                }
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
            //if slot has no item do nothing
            if(Slots[slotIndex].Item == null) return;
            
            var itemTypeName = Slots[slotIndex].Item.ItemData.ItemTypeString;
            if(itemTypeName == "Seed")
            {
                inventoryOwner.GenericStrategy.RemoveItem();
            }
            else if (itemTypeName == "Weapon")
            {
                inventoryOwner.WeaponStrategy.RemoveWeapon();
            }
            else
            {
                Debug.LogWarning("Selected item is not valid.");
            }
            inventoryOwner.UnsetStrategy();
            _selectedItem = null;
        }
    }
}
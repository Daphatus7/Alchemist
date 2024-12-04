using System;
using _Script.Inventory.InventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.ActionBarBackend
{
    public class ActionBar : PlayerInventory
    {
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
                _selectedItem = Slots[slotIndex];
            }
        }
        private int _selectedSlotIndex; public int SelectedSlotIndex => _selectedSlotIndex;
        
        private ActionBarContext _actionBarContext;
        
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
                
                //allowing the item to be used temporarily
                _actionBarContext = new ActionBarContext(LeftClickItem, slotIndex, _selectedItem.ItemData);
                if(itemType == "Seed")
                {
                    inventoryOwner.GenericStrategy.ChangeItem(_actionBarContext);
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
            if(Slots[slotIndex].IsEmpty) return;
            RemoveStrategy(slotIndex);
        }

        private void RemoveStrategy(int slotIndex)
        {
            var itemTypeName = Slots[slotIndex].ItemData.ItemTypeString;
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
        

        protected override void OnItemUsedUp(int slotIndex)
        {
            //if the used up item is the selected item
            if(slotIndex == _selectedSlotIndex)
            {
                _selectedItem = null; 
                RemoveStrategy(slotIndex);
            }
        }
    }

    public class ActionBarContext
    {
        private readonly Action<int> _performAction; // Use Action<int> for a method that takes an int and returns void
        private readonly int _selectedSlotIndex;
        private readonly ItemData _itemData;

        // Expose ItemData through a property
        public ItemData ItemData => _itemData;

        // Constructor accepting Action<int> for slot-specific actions
        public ActionBarContext(Action<int> performAction, int selectedSlotIndex, ItemData itemData)
        {
            _performAction = performAction ?? throw new ArgumentNullException(nameof(performAction));
            _itemData = itemData ?? throw new ArgumentNullException(nameof(itemData));
            _selectedSlotIndex = selectedSlotIndex;
        }

        // UseItem now calls the Action<int> delegate
        public void UseItem()
        {
            _performAction(_selectedSlotIndex);
        }
    }
}
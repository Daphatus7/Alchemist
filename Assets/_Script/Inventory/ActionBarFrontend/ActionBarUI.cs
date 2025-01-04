using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendBase;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.ActionBarFrontend
{
    public class ActionBarUI : InventoryUIBase<PlayerInventory.PlayerInventory>, IPlayerInventoryHandler
    {
        
        private InventorySlotInteraction _selectedSlotInteraction;
        
        public void InitializeInventoryUI(PlayerInventory.PlayerInventory playerInventory, int selectedSlot = 0)
        {
            inventory = playerInventory;
            InitializeInventoryUI();
        }
        
        public new void OnSlotClicked(InventorySlotInteraction slotInteraction)
        {
            Debug.Log("Slot clicked in action bar.");
            SelectSlot(slotInteraction.SlotIndex);
        }
        
        /// <summary>
        /// Select the slot at the given index.
        /// if the slot is already selected, use the item.
        /// if the previous slot is not empty, deselect it.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void SelectSlot(int slotIndex)
        {
            //Case 1: Invalid slot index
            // do nothing
            if (slotIndex < 0 || slotIndex >= _slotInteractions.Length)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }
            
            //Case 2: Check if has an item
            //if the slot is empty, then try to deselect the previous item
            //and set selected item to null
            var itemStack = inventory.GetItemStackAt(slotIndex);
            if(itemStack == null || itemStack.IsEmpty)
            {
                Debug.LogWarning("No item in slot.");
                //still deselect the previous item
                DeselectPreviousSlot();
                _selectedSlotInteraction = _slotInteractions[slotIndex];
                
                inventory.OnSelectNone();
                return;
            }
            
            //Case 3: Check if selecting the same slot and is not seed item
            if (_selectedSlotInteraction && _selectedSlotInteraction.SlotIndex == slotIndex && inventory.GetItemStackAt(slotIndex).ItemData.ItemTypeString != "Seed")
            {
                // Use the selected slot
                inventory.LeftClickItem(slotIndex);
                return;
            }
            
            /*Deselect previous item*/
            // Unhighlight the previous slot
            DeselectPreviousSlot();
            
            /*Select new item*/
            // Highlight the new slot
            SetSelectedSlot(slotIndex);
            // Update selected slot item
        }
        
        private void SetSelectedSlot(int slotIndex)
        {
            _selectedSlotInteraction = _slotInteractions[slotIndex];
            inventory.OnSelectItem(slotIndex);
        }

        
        private void DeselectPreviousSlot()
        {
            if (_selectedSlotInteraction)
            {
                //Visual
                //Logic
                inventory.OnDeSelectItem(_selectedSlotInteraction.SlotIndex);
                _selectedSlotInteraction = null;
            }
        }
        
        #region Keyboard Input

        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Alpha1))
        //     {
        //         SelectSlot(0);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha2))
        //     {
        //         SelectSlot(1);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha3))
        //     {
        //         SelectSlot(2);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha4))
        //     {
        //         SelectSlot(3);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha5))
        //     {
        //         SelectSlot(4);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha6))
        //     {
        //         SelectSlot(5); 
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha7))
        //     {
        //         SelectSlot(6);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha8))
        //     {
        //         SelectSlot(7);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha9))
        //     {
        //         SelectSlot(8);
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Alpha0))
        //     {
        //         SelectSlot(9);
        //     }
        // }


        #endregion
        

        public override ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            //if the slot is selected, deselect it
            if(slotIndex == inventory.SelectedSlotIndex)
            {
                DeselectPreviousSlot();
            }
            return base.RemoveAllItemsFromSlot(slotIndex);
        }

        public override void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            base.AddItemToEmptySlot(itemStack, slotIndex);
            //Debug.Log("Item added to slot " + slotIndex + " in action bar." +_actionBar.SelectedSlotIndex);
            if(slotIndex == inventory.SelectedSlotIndex)
            {
                SelectSlot(slotIndex);
            }
        }
  
        public void AddGold(int amount)
        {
            inventory.InventoryOwner.AddGold(amount);
        }

        public bool RemoveGold(int amount)
        {
            return inventory.InventoryOwner.RemoveGold(amount);
        }
    }
}

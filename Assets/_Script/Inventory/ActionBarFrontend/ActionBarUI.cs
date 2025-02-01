using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendBase;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.ActionBarFrontend
{
    public class ActionBarUI : InventoryUIBase<PlayerInventory.PlayerInventory>, IPlayerInventoryHandler
    {
        private InventorySlotInteraction _selectedSlotInteraction;

        /// <summary>
        /// Initializes the player inventory UI.
        /// </summary>
        /// <param name="playerInventory">The player's inventory instance.</param>
        /// <param name="selectedSlot">Which slot to select by default.</param>
        public void InitializeInventoryUI(PlayerInventory.PlayerInventory playerInventory, int selectedSlot = 0)
        {
            inventory = playerInventory;
            InitializeInventoryUI();

            // Optionally select the initial slot if it’s in valid range
            if (selectedSlot >= 0 && selectedSlot < _slotInteractions.Length)
            {
                SelectSlot(selectedSlot);
            }
        }

        /// <summary>
        /// Override the base OnSlotClicked (if in the base class it’s virtual).
        /// If the base class method is *not* virtual, then use 'new' carefully.
        /// </summary>
        /// <param name="slotInteraction"></param>
        public new void OnSlotClicked(InventorySlotInteraction slotInteraction)
        {
            SelectSlot(slotInteraction.SlotIndex);
        }

        /// <summary>
        /// Selects the slot at the given index.
        /// If the slot is already selected, use the item.
        /// If the previous slot has an item and is different, it gets deselected first.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void SelectSlot(int slotIndex)
        {
            // 1) Validate index
            if (slotIndex < 0 || slotIndex >= _slotInteractions.Length)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            // 2) If there is a previously selected slot (i.e., if inventory.SelectedSlotIndex is valid), 
            //    check if the new slot is different from the previous one and if so, deselect the previous.
            if (inventory.SelectedSlotIndex != -1 && slotIndex != inventory.SelectedSlotIndex)
            {
                var previousItemStack = inventory.GetItemStackAt(inventory.SelectedSlotIndex);
                var newItemStack = inventory.GetItemStackAt(slotIndex);
                if(newItemStack == null)
                {
                    return;
                }
                var selectedItem = newItemStack.ItemData as ConsumableItem;
                if(selectedItem != null)
                {
                    inventory.UseItem(slotIndex);
                    return;
                }
                else if (previousItemStack != null && !previousItemStack.IsEmpty &&
                    previousItemStack != newItemStack)
                {
                    DeselectPreviousSlot();
                }
            }

            // 3) Check if the new slot actually has an item
            var itemStack = inventory.GetItemStackAt(slotIndex);
            if (itemStack == null || itemStack.IsEmpty)
            {
                // If empty, we might want to also deselect any previously selected slot
                // but that depends on your intended behavior.
                // For now, we’ll just do nothing further.
                return;
            }

            // 4) Decide whether to use or equip the item
            //    Usually you'd also want to set the inventory.SelectedSlotIndex = slotIndex here
            inventory.SelectedSlotIndex = slotIndex;

            if (itemStack.ItemData is EquipmentItem)
            {
                // If the item is equipment, we "equip" or "activate" it
                inventory.OnSelectItem(slotIndex);
                // Optionally track which slot UI is selected
            }
            else
            {
                // If the item is consumable, we "use" it
                // In some designs, using the item does *not* necessarily select it
                inventory.UseItem(slotIndex);
                // If we do want to show a "selection" visually, call SetSelectedSlot
            }

            SetSelectedSlot(slotIndex);
        }

        /// <summary>
        /// Sets the currently selected slot in the UI layer.
        /// Also triggers the logical OnSelectItem if not already triggered.
        /// </summary>
        /// <param name="slotIndex"></param>
        private void SetSelectedSlot(int slotIndex)
        {
            _selectedSlotInteraction = _slotInteractions[slotIndex];
        }

        /// <summary>
        /// Deselects the previously selected slot, if any.
        /// </summary>
        private void DeselectPreviousSlot()
        {
            // If we have a valid selected slot
            if (_selectedSlotInteraction)
            {
                // Logic: tell the inventory to deselect that slot
                inventory.OnDeSelectItem(_selectedSlotInteraction.SlotIndex);

                // Clear references
                _selectedSlotInteraction = null;
                inventory.SelectedSlotIndex = -1; 
            }
        }

        #region Keyboard Input
        /*
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectSlot(1);
            }
            // etc...
        }
        */
        #endregion

        public override ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            // If the slot is currently selected, deselect it first
            if (slotIndex == inventory.SelectedSlotIndex)
            {
                DeselectPreviousSlot();
            }
            return base.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddGold(int amount)
        {
            inventory.InventoryOwner.AddGold(amount);
        }

        public bool RemoveGold(int amount)
        {
            return inventory.InventoryOwner.RemoveGold(amount);
        }

        public int GetGold()
        {
            return inventory.InventoryOwner.Gold;
        }

        public override void InventoryUpdated()
        {
            //call player quest manager
        }
    }
}
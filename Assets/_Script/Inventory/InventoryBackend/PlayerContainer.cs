using System;
using UnityEngine;
using _Script.Character;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Inventory.InventoryBackend
{
    /// <summary>
    /// A specialized Inventory class for the Player. Supports shape-based item placement
    /// (via the inherited Inventory logic) and custom "use item" behaviors (equipment, consumables, etc.).
    /// </summary>
    public class PlayerContainer : Inventory
    {
        protected PlayerCharacter inventoryOwner; 
        public PlayerCharacter InventoryOwner => inventoryOwner;

        public override SlotType SlotType => SlotType.PlayerInventory;
        public string UniqueID { get; }
        
        /// <summary>
        /// Initializes an empty PlayerContainer with shape-based slots.
        /// </summary>
        /// <param name="owner">The owning PlayerCharacter.</param>
        /// <param name="height">Number of rows (height) for the grid.</param>
        /// <param name="width">Number of columns (width) for the grid.</param>
        public PlayerContainer(PlayerCharacter owner, int height, int width) 
            : base(height, width)
        {
            UniqueID = Guid.NewGuid().ToString();
            inventoryOwner = owner;
        }

        /// <summary>
        /// Optionally, you can add a "load from items" constructor if you want to restore saved data.
        /// </summary>
        public PlayerContainer(PlayerCharacter owner, int height, int width, ItemStack[] items)
            : base(height, width, items)
        {
            UniqueID = Guid.NewGuid().ToString();
            inventoryOwner = owner;
        }

        /// <summary>
        /// Uniqueness check (e.g., for containers with a UniqueID).
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is PlayerContainer other)
            {
                return this.UniqueID == other.UniqueID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return UniqueID != null ? UniqueID.GetHashCode() : 0;
        }

        // -------------------------------------------------------------------
        // Example usage logic for different item types
        // -------------------------------------------------------------------

        private ItemStack OnUseEquipmentItem(EquipmentItem itemData)
        {
            throw new NotImplementedException("Implement the equip item method");
        }

        private bool OnUseConsumableItem(ConsumableItem itemData)
        {
            // Use the consumable effect on the player
            if (itemData != null) itemData.Use(inventoryOwner);
            return true;
        }

        private ItemStack OnUseMaterialItem(ItemData itemData)
        {
            // Using a material might just call itemData.Use(...) or do crafting, etc.
            if (itemData != null) itemData.Use(inventoryOwner);
            
            return null;
        }

        private bool OnUseSeedItem(ItemData itemData)
        {
            // e.g., plant a seed in the game world, reduce item stack by 1
            if (itemData != null)
            {
                return itemData.Use(inventoryOwner);
            }
            return false;
        }

        /// <summary>
        /// Called when the player "uses" (e.g., right-clicks) an item in a specific slot.
        /// Here we decide how to handle equipment, containers, seeds, consumables, etc.
        /// </summary>
        protected virtual void OnUsingItem(ItemStack slotStack, int slotIndex)
        {
            if (slotStack == null || slotStack.IsEmpty) return;

            // We use the string type name to differentiate. 
            // Alternatively, you could do 'if (slotStack.ItemData.ItemType == ItemType.Consumable)' etc.
            var itemType = slotStack.ItemData.ItemTypeString;

            if (itemType == "Equipment")
            {

                throw new NotImplementedException("Implement the equip item method");
                var eqItem = slotStack.ItemData as EquipmentItem;
                if (eqItem != null)
                {
                    var removedItemStack = OnUseEquipmentItem(eqItem);
                    // If eq system returns an item that was replaced, consider re-inserting it
                    // or removing one from the slot. Up to your design.
                    RemoveItemFromSlot(slotIndex, 1); 
                    if (removedItemStack != null)
                    {
                        // e.g. put it back in inventory
                        AddItem(removedItemStack);
                    }
                }
            }
            else if (itemType == "Container")
            {
                // If it's a container item (like a bag), open it
                if (slotStack is ContainerItemStack conStack)
                {
                    inventoryOwner?.OpenContainerInstance(conStack.AssociatedContainer);
                }
            }
            else if (itemType == "Seed")
            {
                // Attempt to plant
                if (OnUseSeedItem(slotStack.ItemData))
                {
                    // If planting succeeded, remove 1 from slot
                    RemoveItemFromSlot(slotIndex, 1);
                }
            }
            else if (itemType == "Consumable")
            {
                // e.g. potions, food
                var conItem = slotStack.ItemData as ConsumableItem;
                if (conItem != null)
                {
                    if (OnUseConsumableItem(conItem))
                    {
                        RemoveItemFromSlot(slotIndex, 1);
                    }
                }
            }
            else if (itemType == "Material")
            {
                // e.g. place or craft with this material
                OnUseMaterialItem(slotStack.ItemData);
                // Potentially remove or not
                // e.g. RemoveItemFromSlot(slotIndex, 1);
            }
            // else: handle other item types, or do nothing
        }

        /// <summary>
        /// When we left-click on a slot, we attempt to "use" the item there.
        /// Could also handle e.g. dragging logic, but that might be in the UI layer.
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            ItemStack slotStack = Slots[slotIndex].ItemStack;
            if (slotStack.IsEmpty)
            {
                // e.g. no item to use
                return false;
            }
            
            OnUsingItem(slotStack, slotIndex);
            return true;
        }

        /// <summary>
        /// The required override from the abstract parent class:
        /// we define how to handle "LeftClickItem".
        /// Here we simply call 'UseItem(slotIndex)'.
        /// </summary>
        public override void LeftClickItem(int slotIndex)
        {
            UseItem(slotIndex);
        }
        
    }
}
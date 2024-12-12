using _Script.Character;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public class PlayerContainer : Inventory
    {
        protected PlayerCharacter inventoryOwner; public PlayerCharacter InventoryOwner => inventoryOwner;

        public override SlotType SlotType => SlotType.PlayerInventory;
        public string UniqueID { get; }
        

        /// <summary>
        /// Initializes an empty inventory.
        /// </summary>
        public PlayerContainer(PlayerCharacter owner, int capacity) : base(capacity)
        {
            UniqueID = System.Guid.NewGuid().ToString();
            inventoryOwner = owner;
        }
        
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
        
        private ItemStack OnUseEquipmentItem(EquipmentItem itemData)
        {
            // Logic for equipping the item to the player.
            // Returns any item stack that needs to be reinserted into the inventory if applicable.
            return inventoryOwner.PlayerEquipment.Handle_Equip(itemData);
        }

        private bool OnUseConsumableItem(ConsumableItem itemData)
        {
            // Uses the consumable item effect on the player.
            itemData.Use(inventoryOwner);
            return true;
        }

        private ItemStack OnUseMaterialItem(ItemData itemData)
        {
            // Logic for using a material item if needed.
            // Currently just invokes Use() and returns null.
            itemData.Use(inventoryOwner);
            return null;
        }

        private bool OnUseSeedItem(ItemData itemData)
        {
            // Logic for using a seed item, e.g., planting a crop in the world.
            return itemData.Use(inventoryOwner);
        }

        /// <summary>
        /// This method is invoked when the player uses (e.g., right-clicks) an item in the inventory slot.
        /// 1. Determines the item type.
        /// 2. Applies the corresponding effect.
        /// 3. Removes or decreases the item stack if necessary.
        /// </summary>
        protected virtual void OnUsingItem(ItemStack slot, int slotIndex)
        {
            var itemType = slot.ItemData.ItemTypeString;

            if (itemType == "Equipment")
            {
                // Equipment usage logic is currently commented out.
                // Example (disabled):
                // var removedItemStack = OnUseEquipmentItem((EquipmentItem)itemData);
                // RemoveItemFromSlot(slotIndex, 1);
                // if (removedItemStack != null)
                // {
                //     AddItemToSlot(removedItemStack, slotIndex);
                // }
                return;
            }
            else if (itemType == "Container")
            {
                // Opens a container-type item (like a chest or bag).
                if (slot is ContainerItemStack con) 
                    inventoryOwner.OpenContainerInstance(con.AssociatedContainer);
            }
            else if (itemType == "Seed")
            {
                // Uses a seed item. If successful, remove one from the slot.
                if (OnUseSeedItem(slot.ItemData))
                {
                    RemoveItemFromSlot(slotIndex, 1);
                }
            }
            else if (itemType == "Consumable")
            {
                // Uses a consumable item. If successful, remove one from the slot.
                if (OnUseConsumableItem((ConsumableItem)slot.ItemData))
                {
                    RemoveItemFromSlot(slotIndex, 1);
                }
            }
            else if (itemType == "Material")
            {
                // Uses a material item. No guaranteed removal logic; depends on the effect.
                OnUseMaterialItem(slot.ItemData);
            }
        }

        /// <summary>
        /// Attempts to use the item in the given slot index (e.g., left-click action).
        /// </summary>
        protected bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                //Debug.LogWarning("Invalid slot index.");
                return false;
            }

            ItemStack slot = Slots[slotIndex];
            if (slot.IsEmpty)
            {
                //Debug.Log("Slot is empty.");
                return false;
            }
            
            OnUsingItem(slot, slotIndex);
            return true;
        }

        public override void LeftClickItem(int slotIndex)
        {
            // Here, LeftClickItem logic can vary based on the game rules.
            // Currently, we simply use the item in that slot.
            UseItem(slotIndex);
        }
    }
}
using _Script.Character;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public class PlayerInventory : Inventory
    {
        protected PlayerCharacter inventoryOwner; public PlayerCharacter InventoryOwner => inventoryOwner;
        

        public override SlotType SlotType => SlotType.PlayerInventory;

        protected override void Awake()
        {
            base.Awake();
            inventoryOwner = GetComponentInParent<PlayerCharacter>();
        }

        public bool Handle_RemoveItem(InventoryItem inventoryItem)
        {
            return RemoveItem(inventoryItem);
        }
        
        private InventoryItem OnUseEquipmentItem(EquipmentItem itemData)
        {
            // Equip the item
            return inventoryOwner.PlayerEquipment.Handle_Equip(itemData);
        }
        private bool OnUseConsumableItem(ConsumableItem itemData)
        {
            itemData.Use(inventoryOwner);
            return true;
        }
        private InventoryItem OnUseMaterialItem(ItemData itemData)
        {
            itemData.Use(inventoryOwner);
            return null;
        }
        
        private bool OnUseSeedItem(ItemData itemData)
        {
            return itemData.Use(inventoryOwner);
        }
        
        
        
        /**
         * When right-clicking on an inventory item.
         * 1. Put the item in temporary slot
         * 2. Get the type of the item
         * 3. Apply the effect of the item to the player
         * 4. Remove the item from the inventory
         */
        protected virtual void OnUsingItem(ItemData itemData, int slotIndex)
        {
            // Implement item usage logic
            
            //Use Equipment Item - if there is item in the equipment inventory, remove it and add it back to the inventory
            var itemType = itemData.ItemTypeString;
            
            
            if(itemType == "Equipment")
            {
                Debug.Log("Using Equipment Item Currently Disabled");
                return;
                InventoryItem removedItem = OnUseEquipmentItem((EquipmentItem) itemData);
                RemoveItemFromSlot(slotIndex, 1);
                if(removedItem != null)
                {
                    // Remove the item from the inventory
                    // Add the removed item back to the inventory
                    AddItemToSlot(removedItem, slotIndex);
                }
            }
            else if (itemType == "Seed")
            {
                if(OnUseSeedItem(itemData))
                {
                    Debug.Log("Seed item used.");
                    // Remove the item from the inventory
                    RemoveItemFromSlot(slotIndex, 1);
                }
            }
            //Use Consumable Item - if the item is used, remove it from the inventory
            else if(itemType == "Consumable")
            {
                if (OnUseConsumableItem((ConsumableItem)itemData))
                {
                    // Remove the item from the inventory
                    RemoveItemFromSlot(slotIndex, 1);
                }
            }
            //Use Material Item
            else if(itemType == "Material")
            {
                OnUseMaterialItem(itemData);
                Debug.Log("There is no effect for using material item.");
            }
        }
        
        /**
         * When right-clicking on an inventory item.
         */
        private bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            InventoryItem slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                //Debug.Log("Slot is empty.");
                return false;
            }

            ItemData itemData = slot.ItemData;

            OnUsingItem(itemData, slotIndex);
            return true;
        }
        
        public override void LeftClickItem(int slotIndex)
        {
            UseItem(slotIndex);
        }
    }
}
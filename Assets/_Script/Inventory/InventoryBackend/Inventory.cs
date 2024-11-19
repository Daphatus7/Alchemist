using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public abstract class Inventory : MonoBehaviour
    {
        [SerializeField] private int capacity = 20;

        private PlayerCharacter inventoryOwner; public PlayerCharacter InventoryOwner => inventoryOwner;
        public int Capacity => capacity;

        // Fixed-size array representing inventory slots
        private InventorySlot[] slots; public InventorySlot[] Slots => slots;

        // Event to notify when the inventory has changed
        public event Action OnInventoryChanged;

        public void SetInventoryOwner(PlayerCharacter playerInventoryCharacter)
        {
            inventoryOwner = playerInventoryCharacter;
        }
        
        private void Awake()
        {
            //private 
            
            // Initialize the slots array with the capacity
            inventoryOwner = GetComponentInParent<PlayerCharacter>();
            slots = new InventorySlot[capacity];
            for (int i = 0; i < capacity; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        protected bool AddItem(InventoryItem itemToAdd)
        {
            if (itemToAdd == null)
            {
                Debug.LogWarning("ItemData is null.");
                return false;
            }

            int quantityToAdd = itemToAdd.Quantity;

            // First, try to add to existing stacks with the same item that are not full
            for (int i = 0; i < capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.Item.ItemData == itemToAdd.ItemData && slot.Item.Quantity < itemToAdd.ItemData.MaxStackSize)
                {
                    int availableSpace = itemToAdd.ItemData.MaxStackSize - slot.Item.Quantity;
                    int amountToAdd = Math.Min(quantityToAdd, availableSpace);
                    slot.Item.Quantity += amountToAdd;
                    quantityToAdd -= amountToAdd;

                    // Notify UI update
                    OnInventoryChanged?.Invoke();

                    if (quantityToAdd <= 0)
                    {
                        return true;
                    }
                }
            }

            // Next, try to add to empty slots
            for (int i = 0; i < capacity; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty)
                {
                    int amountToAdd = Math.Min(quantityToAdd, itemToAdd.ItemData.MaxStackSize);
                    slot.Item = new InventoryItem(itemToAdd.ItemData, amountToAdd);
                    quantityToAdd -= amountToAdd;

                    // Notify UI update
                    OnInventoryChanged?.Invoke();

                    if (quantityToAdd <= 0)
                    {
                        return true;
                    }
                }
            }

            if (quantityToAdd > 0)
            {
                Debug.Log("Inventory is full!");
                return false;
            }

            return true;
        }

        protected bool RemoveItem(InventoryItem inventoryItem, int quantity = 1)
        {
            int quantityToRemove = quantity;

            // Go through the slots and remove items
            for (int i = 0; i < capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.Item.ItemData == inventoryItem.ItemData)
                {
                    int amountToRemove = Math.Min(quantityToRemove, slot.Item.Quantity);
                    slot.Item.Quantity -= amountToRemove;
                    quantityToRemove -= amountToRemove;

                    // If the quantity reaches zero, clear the slot's item data, but keep the slot
                    if (slot.Item.Quantity <= 0)
                    {
                        slot.Clear();
                    }

                    // Notify UI update
                    OnInventoryChanged?.Invoke();

                    if (quantityToRemove <= 0)
                    {
                        return true;
                    }
                }
            }

            if (quantityToRemove > 0)
            {
                Debug.Log("Not enough items to remove.");
                return false;
            }

            return true;
        }
        

        /**
         * When right-clicking on an inventory item.
         */
        private void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return;
            }

            ItemData itemData = slot.Item.ItemData;

            OnUsingItem(itemData, slotIndex);
        }
        
        protected virtual void OnUsingItem(ItemData itemData, int slotIndex)
        {
            // Implement item usage logic
            
            bool itemUsed = RemoveItemFromSlot(slotIndex, 1);
            
            //add item to equipment inventory
            var equipmentInventory = inventoryOwner.GetPlayerEquipment();
            if(equipmentInventory == null)
            {
                Debug.LogWarning("Equipment inventory is null.");
                return;
            }
            inventoryOwner.GetPlayerEquipment().Handle_EquipItem(new InventoryItem(itemData));
            if (itemUsed)
            {
                itemData.Use(inventoryOwner);
                Debug.Log($"Used item: {itemData.ItemName}");
                // Implement additional logic based on item effects
            }
            else
            {
                Debug.Log("Failed to use item.");
            }
        }

        private InventoryItem OnUseEquipmentItem(ItemData itemData)
        {
            return null;
        }
        private InventoryItem OnUseConsumableItem(ItemData itemData)
        {
            return null;
        }
        private InventoryItem OnUseMaterialItem(ItemData itemData)
        {
            return null;
        }
        
        
        private bool RemoveItemFromSlot(int slotIndex, int quantity)
        {
            var slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return false;
            }

            if (slot.Item.Quantity >= quantity)
            {
                slot.Item.Quantity -= quantity;

                // If quantity reaches zero, clear the slot
                if (slot.Item.Quantity <= 0)
                {
                    slot.Clear();
                }

                // Notify UI update
                OnInventoryChanged?.Invoke();

                return true;
            }
            else
            {
                Debug.Log("Not enough items in slot to remove.");
                return false;
            }
        }

        public void LeftClickItem(int slotIndex)
        {
            UseItem(slotIndex);
        }
    }

    public class InventorySlot
    {
        public InventoryItem Item { get; set; }

        public bool IsEmpty => Item is not { Quantity: > 0 };

        public void Clear()
        {
            Item = null;
        }
    }
}

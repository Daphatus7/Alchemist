using System;
using System.Collections.Generic;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public abstract class Inventory : MonoBehaviour
    {
        [SerializeField] private int capacity = 20;
        public int Capacity => capacity;

        // Fixed-size array representing inventory slots
        [SerializeField] private InventorySlot[] slots; public InventorySlot[] Slots => slots;

        // Event to notify when the inventory has changed
        public event Action OnInventoryChanged;

        private void Awake()
        {
            // Initialize the slots array with the capacity
            slots = new InventorySlot[capacity];
            for (int i = 0; i < capacity; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        protected bool AddItem(ItemData itemData, int quantity)
        {
            if (itemData == null)
            {
                Debug.LogWarning("ItemData is null.");
                return false;
            }

            int quantityToAdd = quantity;

            // First, try to add to existing stacks with the same item that are not full
            for (int i = 0; i < capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.Item.ItemData == itemData && slot.Item.Quantity < itemData.MaxStackSize)
                {
                    int availableSpace = itemData.MaxStackSize - slot.Item.Quantity;
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
                    int amountToAdd = Math.Min(quantityToAdd, itemData.MaxStackSize);
                    slot.Item = new InventoryItem(itemData, amountToAdd);
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

        protected bool RemoveItem(ItemData itemData, int quantity)
        {
            int quantityToRemove = quantity;

            // Go through the slots and remove items
            for (int i = 0; i < capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.Item.ItemData == itemData)
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

        protected void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            var slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return;
            }

            var itemData = slot.Item.ItemData;

            // Implement item usage logic
            bool itemUsed = RemoveItemFromSlot(slotIndex, 1);
            if (itemUsed)
            {
                Debug.Log($"Used item: {itemData.ItemName}");
                // Implement additional logic based on item effects
            }
            else
            {
                Debug.Log("Failed to use item.");
            }
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

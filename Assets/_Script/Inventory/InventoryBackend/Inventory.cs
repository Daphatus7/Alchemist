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

        public int Capacity => capacity;

        // Fixed-size array representing inventory slots
        protected InventorySlot[] slots; public InventorySlot[] Slots => slots;

        // Event to notify when the inventory has changed
        public event Action<int> OnInventorySlotChanged;
        
        
        protected virtual void Awake()
        {
            //private 
            
            // Initialize the slots array with the capacity
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
                    OnInventorySlotChanged?.Invoke(i);

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
                    OnInventorySlotChanged?.Invoke(i);

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
        
        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            if (slots[slotIndex].IsEmpty)
            {
                slots[slotIndex].Item = item;

                // Notify UI update
                OnInventorySlotChanged?.Invoke(slotIndex);
            }
        }

        protected bool AddItemToSlot(InventoryItem itemToAdd, int slotIndex)
        {
            if (itemToAdd == null)
            {
                Debug.LogWarning("ItemData is null.");
                return false;
            }

            if (slotIndex < 0 || slotIndex >= capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            var slot = slots[slotIndex];
            if (!slot.IsEmpty && slot.Item.ItemData == itemToAdd.ItemData && slot.Item.Quantity < itemToAdd.ItemData.MaxStackSize)
            {
                int availableSpace = itemToAdd.ItemData.MaxStackSize - slot.Item.Quantity;
                int amountToAdd = Math.Min(itemToAdd.Quantity, availableSpace);
                slot.Item.Quantity += amountToAdd;

                // Notify UI update
                OnInventorySlotChanged?.Invoke(slotIndex);
                
                return true;
            }

            if (slot.IsEmpty)
            {
                int amountToAdd = Math.Min(itemToAdd.Quantity, itemToAdd.ItemData.MaxStackSize);
                slot.Item = new InventoryItem(itemToAdd.ItemData, amountToAdd);

                // Notify UI update
                OnInventorySlotChanged?.Invoke(slotIndex);

                return true;
            }

            Debug.Log("Slot is full.");
            return false;
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }

            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                return null;
            }

            InventoryItem item = slot.Item;
            slot.Clear();

            // Notify UI update
            OnInventorySlotChanged?.Invoke(slotIndex);

            return item;
        }
        
        protected virtual bool RemoveItem(InventoryItem inventoryItem, int quantity = 1)
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
                        OnItemUsedUp(i);
                        slot.Clear();
                    }

                    // Notify UI update
                    OnInventorySlotChanged?.Invoke(i);

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
        




        protected bool RemoveItemFromSlot(int slotIndex, int quantity)
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
                    OnItemUsedUp(slotIndex);
                    slot.Clear();
                }

                // Notify UI update
                OnInventorySlotChanged?.Invoke(slotIndex);
                return true;
            }
            else
            {
                Debug.Log("Not enough items in slot to remove.");
                return false;
            }
        }
        
        protected virtual void OnItemUsedUp(int slotIndex)
        {
        }

        public abstract void LeftClickItem(int slotIndex);

    }

    public class InventorySlot
    {
        public InventoryItem Item { get; set; }

        public bool IsEmpty => Item is not { Quantity: > 0 };

        public void Clear()
        {
            Item = null;
        }
        
        public bool IsEmptySlot()
        {
            return Item == null;
        }
    }
    
    public enum InventoryType
    {
        Inventory,
        Equipment,
        Crafting,
    }
}

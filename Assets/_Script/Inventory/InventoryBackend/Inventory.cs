using System;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public abstract class Inventory
    {
        private readonly int _capacity = 20; public int Capacity => _capacity;
        protected InventoryItem[] slots; public InventoryItem[] Slots => slots;
        
        //Load an Empty Inventory
        public Inventory(int capacity)
        {
            _capacity = capacity;
            
            slots = new InventoryItem[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                slots[i] = new InventoryItem();
            }
        }
        
        //Load an Inventory with Items
        public Inventory(int capacity, InventoryItem[] items)
        {
            _capacity = capacity;
            slots = items;
            
            slots = new InventoryItem[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                slots[i] = new InventoryItem();
            }
        }
        

        public abstract SlotType SlotType { get; }

        // Event to notify when the inventory has changed
        public event Action<int> OnInventorySlotChanged;
        

        public InventoryItem AddItem(InventoryItem itemToAdd)
        {
            if (itemToAdd == null)
            {
                //Debug.LogWarning("ItemData is null.");
                return null;
            }

            int quantityToAdd = itemToAdd.Quantity;

            // First, try to add to existing stacks with the same item that are not full
            for (int i = 0; i < _capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemData == itemToAdd.ItemData && slots[i].Quantity < itemToAdd.ItemData.MaxStackSize)
                {
                    int availableSpace = itemToAdd.ItemData.MaxStackSize - slots[i].Quantity;
                    int amountToAdd = Math.Min(quantityToAdd, availableSpace);
                    slots[i].Quantity += amountToAdd;
                    quantityToAdd -= amountToAdd;
                    
                    // Notify UI update
                    OnInventorySlotChanged?.Invoke(i);

                    // If the quantity reaches zero, we are done
                    if (quantityToAdd <= 0)
                    {
                        return null;
                    }
                }
            }

            // Next, try to add to empty slots
            for (int i = 0; i < _capacity; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int amountToAdd = Math.Min(quantityToAdd, itemToAdd.ItemData.MaxStackSize);
                    slots[i] = new InventoryItem(itemToAdd.ItemData, amountToAdd);
                    quantityToAdd -= amountToAdd;

                    // Notify UI update
                    OnInventorySlotChanged?.Invoke(i);

                    if (quantityToAdd <= 0)
                    {
                        return null;
                    }
                }
            }

            if (quantityToAdd > 0)
            {
                //Debug.Log("Inventory is full!");
                return new InventoryItem(itemToAdd.ItemData, quantityToAdd);
            }

            return null;
        }
        
        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                //Debug.LogWarning("Invalid slot index.");
                return;
            }

            if (slots[slotIndex].IsEmpty)
            {
                slots[slotIndex] = item;

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

            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            if (!slots[slotIndex].IsEmpty && slots[slotIndex].ItemData == itemToAdd.ItemData && slots[slotIndex].Quantity < itemToAdd.ItemData.MaxStackSize)
            {
                int availableSpace = itemToAdd.ItemData.MaxStackSize - slots[slotIndex].Quantity;
                int amountToAdd = Math.Min(itemToAdd.Quantity, availableSpace);
                slots[slotIndex].Quantity += amountToAdd;

                // Notify UI update
                OnInventorySlotChanged?.Invoke(slotIndex);
                
                return true;
            }

            if (slots[slotIndex].IsEmpty)
            {
                int amountToAdd = Math.Min(itemToAdd.Quantity, itemToAdd.ItemData.MaxStackSize);
                slots[slotIndex] = new InventoryItem(itemToAdd.ItemData, amountToAdd);

                // Notify UI update
                OnInventorySlotChanged?.Invoke(slotIndex);

                return true;
            }
            Debug.Log("Slot is full.");
            return false;
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }

            if (slots[slotIndex].IsEmpty)
            {
                return null;
            }

            InventoryItem item = new InventoryItem(slots[slotIndex].ItemData, slots[slotIndex].Quantity);
            slots[slotIndex].Clear();

            // Notify UI update
            OnInventorySlotChanged?.Invoke(slotIndex);

            return item;
        }
        
        protected virtual bool RemoveItem(InventoryItem inventoryItem, int quantity = 1)
        {
            int quantityToRemove = quantity;

            // Go through the slots and remove items
            for (int i = 0; i < _capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemData == inventoryItem.ItemData)
                {
                    int amountToRemove = Math.Min(quantityToRemove, slots[i].Quantity);
                    slots[i].Quantity -= amountToRemove;
                    quantityToRemove -= amountToRemove;

                    // If the quantity reaches zero, clear the slot's item data, but keep the slot
                    if (slots[i].Quantity <= 0)
                    {
                        OnItemUsedUp(i);
                        slots[i].Clear();
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
            if (slots[slotIndex].IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return false;
            }

            if (slots[slotIndex].Quantity >= quantity)
            {
                slots[slotIndex].Quantity -= quantity;

                // If quantity reaches zero, clear the slot
                if (slots[slotIndex].Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    slots[slotIndex].Clear();
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
}

using System;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public abstract class Inventory
    {
        private readonly int _capacity;
        public int Capacity => _capacity;
        
        protected readonly ItemStack[] slots;
        public ItemStack[] Slots => slots;
        
        // Load an empty inventory
        public Inventory(int capacity)
        {
            _capacity = capacity;
            
            slots = new ItemStack[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                slots[i] = new ItemStack();
            }
        }
        
        // Load an inventory with items
        public Inventory(int capacity, ItemStack[] items)
        {
            _capacity = capacity;
            slots = new ItemStack[_capacity];

            for (int i = 0; i < _capacity; i++)
            {
                if (items != null && i < items.Length && items[i] != null && !items[i].IsEmpty)
                {
                    // Use CreateStack to preserve special data if it's a specialized stack
                    slots[i] = CreateStack(items[i].ItemData, items[i].Quantity, items[i]);
                }
                else
                {
                    slots[i] = new ItemStack();
                }
            }
        }

        public abstract SlotType SlotType { get; }

        // Event to notify when the inventory has changed
        public event Action<int> OnInventorySlotChanged;
        
        /// <summary>
        /// Attempts to add an item stack to the inventory:
        /// 1. Merge into existing stacks first.
        /// 2. If not fully merged, try placing into empty slots.
        /// Returns null if fully placed, or the leftover if not enough space.
        /// </summary>
        public ItemStack AddItem(ItemStack itemStackToAdd)
        {
            if (itemStackToAdd == null || itemStackToAdd.IsEmpty)
            {
                return null;
            }
            
            // First, try merging with existing stacks
            for (int i = 0; i < _capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.ItemData == itemStackToAdd.ItemData && slot.Quantity < slot.ItemData.MaxStackSize)
                {
                    int oldQuantity = itemStackToAdd.Quantity;
                    int remaining = slot.TryAdd(itemStackToAdd);
                    itemStackToAdd.Quantity = remaining; 
                    
                    if (remaining < oldQuantity)
                        OnInventorySlotChanged?.Invoke(i);

                    if (remaining == 0)
                    {
                        return null; // Fully merged
                    }
                }
            }

            // Then, try placing into empty slots
            for (int i = 0; i < _capacity; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);
                    
                    // Preserve data by creating a stack of the same type
                    slots[i] = CreateStack(itemStackToAdd.ItemData, toAdd, itemStackToAdd);
                    OnInventorySlotChanged?.Invoke(i);

                    itemStackToAdd.Quantity -= toAdd; 
                    if (itemStackToAdd.Quantity <= 0)
                    {
                        return null; // Fully placed
                    }
                }
            }

            // Not enough space
            return itemStackToAdd;
        }
        
        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            if (slots[slotIndex].IsEmpty)
            {
                // Use CreateStack to preserve item stack type and data
                slots[slotIndex] = CreateStack(itemStack.ItemData, itemStack.Quantity, itemStack);
                OnInventorySlotChanged?.Invoke(slotIndex);
            }
            else
            {
                Debug.LogWarning("Target slot is not empty, cannot add item directly.");
            }
        }

        protected bool AddItemToSlot(ItemStack itemStackToAdd, int slotIndex)
        {
            if (itemStackToAdd == null || itemStackToAdd.IsEmpty)
            {
                Debug.LogWarning("ItemData is null or stack is empty.");
                return false;
            }

            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            var slot = slots[slotIndex];
            if (!slot.IsEmpty && slot.ItemData == itemStackToAdd.ItemData && slot.Quantity < itemStackToAdd.ItemData.MaxStackSize)
            {
                int remaining = slot.TryAdd(itemStackToAdd);
                OnInventorySlotChanged?.Invoke(slotIndex);
                return remaining < itemStackToAdd.Quantity; 
            }

            if (slot.IsEmpty)
            {
                int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);
                slots[slotIndex] = CreateStack(itemStackToAdd.ItemData, toAdd, itemStackToAdd);
                OnInventorySlotChanged?.Invoke(slotIndex);

                itemStackToAdd.Quantity -= toAdd;
                return toAdd > 0;
            }

            Debug.Log("Slot is full or not compatible with the item type.");
            return false;
        }

        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
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

            // Create a stack of the same type as the slot for consistency
            ItemStack removed = CreateStack(slots[slotIndex].ItemData, slots[slotIndex].Quantity, slots[slotIndex]);
            slots[slotIndex].Clear();
            OnInventorySlotChanged?.Invoke(slotIndex);
            return removed;
        }
        
        protected virtual bool RemoveItem(ItemStack itemStack, int quantity = 1)
        {
            if (itemStack == null || itemStack.IsEmpty || quantity <= 0)
            {
                Debug.LogWarning("Cannot remove null or empty stack, or invalid quantity.");
                return false;
            }

            int quantityToRemove = quantity;
            for (int i = 0; i < _capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemData == itemStack.ItemData)
                {
                    int amountToRemove = Math.Min(quantityToRemove, slots[i].Quantity);
                    slots[i].Quantity -= amountToRemove;
                    quantityToRemove -= amountToRemove;

                    if (slots[i].Quantity <= 0)
                    {
                        OnItemUsedUp(i);
                        slots[i].Clear();
                    }
                    OnInventorySlotChanged?.Invoke(i);

                    if (quantityToRemove <= 0)
                    {
                        return true;
                    }
                }
            }

            Debug.Log("Not enough items to remove.");
            return false;
        }
        
        protected bool RemoveItemFromSlot(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            if (slots[slotIndex].IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return false;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove.");
                return false;
            }

            if (slots[slotIndex].Quantity >= quantity)
            {
                slots[slotIndex].Quantity -= quantity;

                if (slots[slotIndex].Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    slots[slotIndex].Clear();
                }

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
            // Subclasses can override this method to implement special logic when an item stack is used up
        }

        /// <summary>
        /// The logic for clicking an item slot (for example, left-clicking) should be implemented in the subclass.
        /// </summary>
        public abstract void LeftClickItem(int slotIndex);

        /// <summary>
        /// Creates a new stack of the appropriate type (e.g., ContainerItemStack if needed) 
        /// using the given item data, quantity, and template stack.
        /// This ensures that if the template is a specialized stack (like ContainerItemStack),
        /// we preserve that data.
        /// </summary>
        protected ItemStack CreateStack(ItemData itemData, int quantity, ItemStack template)
        {
            // Attempt to cast once
            var cItem = itemData as ContainerItem;
            var cStack = template as ContainerItemStack;

            if (cItem && cStack != null)
            {
                // Both itemData is a ContainerItem and template is a ContainerItemStack
                // Preserve the container data from cStack
                return new ContainerItemStack(cItem, quantity, cStack.AssociatedContainer);
            }
            else if (cItem)
            {
                // itemData is a ContainerItem but the template is not a ContainerItemStack
                // Create a new ContainerItemStack with a fresh container
                return new ContainerItemStack(cItem, quantity, new PlayerContainer(null, cItem.Capacity));
            }
            else if (cStack != null)
            {
                // The template is a ContainerItemStack, but itemData is no longer a ContainerItem.
                // This scenario is unusual; fallback to a normal ItemStack to avoid invalid data.
                Debug.LogWarning("Template was ContainerItemStack but itemData is not ContainerItem. Using normal ItemStack fallback.");
                return new ItemStack(itemData, quantity);
            }
            else
            {
                // Neither condition applies, create a normal ItemStack
                return new ItemStack(itemData, quantity);
            }
        }
    }
}
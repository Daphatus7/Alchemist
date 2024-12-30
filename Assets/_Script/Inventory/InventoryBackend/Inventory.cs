using System;
using System.Collections.Generic;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public class InventorySlot
    {
        private ItemStack _itemStack = null;

        public bool IsEmpty => _itemStack == null || _itemStack.IsEmpty;

        /// <summary>
        /// Get or set the entire ItemStack in this slot.
        /// Setting to null or an empty stack means the slot is empty.
        /// </summary>
        public ItemStack ItemStack
        {
            get => _itemStack;
            set => _itemStack = value;
        }

        /// <summary>
        /// Convenience property for referencing the slot's item data directly.
        /// </summary>
        public ItemData ItemData => _itemStack?.ItemData;
        

        /// <summary>
        /// Clears out the slot, making it empty.
        /// </summary>
        public void Clear()
        {
            _itemStack = null;
        }
    }
    
    public abstract class Inventory
    {
        private readonly int _height; public int Height => _height;
        private readonly int _width; public int Width => _width;

        public int Capacity => _height * _width;

        /**
         * The shape-based slots in the inventory.
         * Each slot is an InventorySlot that may or may not contain an ItemStack.
         */
        protected private InventorySlot[] slots;
        
        public int SlotCount => slots.Length;
        
        public ItemStack GetItemStackAt(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }
            return slots[index].ItemStack;
        }

        /**
         * A parallel array used for partial stack merges (linear approach).
         * Some designs unify this with 'slots', but here we keep them separate
         * for demonstration: e.g., `_itemStacks[i]` matches `slots[i].ItemStack`.
         */
        protected readonly List<ItemStack> _itemStacks;

        public List<ItemStack> ItemStacks => _itemStacks;

        // ----------------------------------------------
        // Constructors
        // ----------------------------------------------

        /// <summary>
        /// Load an empty inventory with given height, width.
        /// </summary>
        public Inventory(int height, int width)
        {
            _height = height;
            _width = width;

            slots = new InventorySlot[Capacity];
            _itemStacks = new List<ItemStack>();

            // Initialize slots
            for (int i = 0; i < Capacity; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        /// <summary>
        /// Load an inventory with items (restoring from save, etc.)
        /// </summary>
        public Inventory(int height, int width, ItemStack[] itemStack)
        {
            _height = height;
            _width = width;

            slots = new InventorySlot[Capacity];
            _itemStacks = new List<ItemStack>();

            // Initialize slots
            for (int i = 0; i < Capacity; i++)
            {
                slots[i] = new InventorySlot();
            }
            
            // Load items
            foreach (var item in itemStack)
            {
                AddItem(item);
            }
        }

        // ----------------------------------------------
        // Abstract properties / events
        // ----------------------------------------------
        public abstract SlotType SlotType { get; }

        // Event: inventory changed at a slot index
        public event Action<int> OnInventorySlotChanged;

        // Helper method to invoke the event
        public void OnInventorySlotChangedEvent(int slotIndex)
        {
            OnInventorySlotChanged?.Invoke(slotIndex);
        }

        // ----------------------------------------------
        // Public API: AddItem
        // ----------------------------------------------

        /// <summary>
        /// Attempts to add an item stack to the inventory:
        /// 1. Merge into existing stacks first (partial stack logic).
        /// 2. If not fully merged, try placing into empty slots via shape-based logic.
        /// Returns null if fully placed, or the leftover if not enough space.
        /// </summary>
        public ItemStack AddItem(ItemStack itemStackToAdd)
        {
            if (itemStackToAdd == null || itemStackToAdd.IsEmpty)
            {
                return null;
            }

            // 1) Merge with existing partial stacks
            for (int i = 0; i < _itemStacks.Capacity; i++)
            {
                var existingStack = _itemStacks[i];
                if (!existingStack.IsEmpty &&
                    existingStack.ItemData == itemStackToAdd.ItemData &&
                    existingStack.Quantity < existingStack.ItemData.MaxStackSize)
                {
                    int oldQuantity = itemStackToAdd.Quantity;
                    int remaining   = existingStack.TryAdd(itemStackToAdd);
                    itemStackToAdd.Quantity = remaining;

                    if (remaining < oldQuantity)
                    {
                        // Some merging happened
                        OnInventorySlotChangedEvent(i);
                    }
                    
                    if (remaining == 0)
                    {
                        // Fully merged
                        return null;
                    }
                }
            }

            // 2) Shape-based placement for leftover
            for (var i = 0; i < Capacity; i++)
            {
                //checking if the item can fit in the inventory
                if (CanFitIn(SlotIndexToGrid(i), itemStackToAdd.ItemData.ItemShape, out List<Vector2Int> requiredSlots))
                {
                    // Decide how many to place
                    int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);

                    // Actually place the item
                    ItemStack placedStack = CreateItemStackAtLocation(requiredSlots, i, itemStackToAdd.ItemData, toAdd, itemStackToAdd);
                    if (placedStack != null)
                    {
                        // Adjust leftover
                        itemStackToAdd.Quantity -= toAdd;

                        if (itemStackToAdd.Quantity <= 0)
                        {
                            // Done
                            return null;
                        }
                    }
                    // If placement failed, continue searching
                }
            }

            // 3) Not enough space
            return itemStackToAdd;
        }
        
        // ----------------------------------------------
        // Checking shape fit
        // ----------------------------------------------

        /// <summary>
        /// Checks if the specified shape can fit, with top-left corner at 'pos' (in grid coords),
        /// by verifying every offset is in range and the slot is empty.
        /// </summary>
        private bool CanFitIn(Vector2Int pos //is the location of the cursor selected slot
            , ItemShape shape, out List<Vector2Int> requiredSlots)
        {
            
            //Get Correct Slot Index
            //Get pivot of the shape
            requiredSlots = new List<Vector2Int>();
            
            foreach (var offset in shape.Positions)
            {
                int gx = pos.x + offset.x;
                int gy = pos.y + offset.y;

                // Check bounds
                if (gx < 0 || gx >= _width || gy < 0 || gy >= _height)
                {
                    return false;
                }

                int finalIndex = GridToSlotIndex(gx, gy);
                if (!slots[finalIndex].IsEmpty)
                {
                    return false;
                }

                requiredSlots.Add(new Vector2Int(gx, gy));
            }
            
            return true;
        }


        public bool CanFitItem(int mySlot, ItemStack comparingItemStack)
        {
            if(mySlot < 0 || mySlot >= Capacity)
            {
                return false;
            }
            
            if (comparingItemStack == null || comparingItemStack.IsEmpty)
            {
                Debug.Log("Invalid item stack to compare.");
                return false;
            }
            
            Debug.Log("Cp " + SlotIndexToGrid(mySlot));
            
            return CanFitIn(SlotIndexToGrid(mySlot), comparingItemStack.ItemData.ItemShape, out List<Vector2Int> requiredSlots);    
        }
        
        // ----------------------------------------------
        // Remove entire stack from a slot
        // ----------------------------------------------
        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            //invalid slot index
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                return null;
            }
            if (GetItemStackAt(slotIndex) == null)
            {
                return null;
            }
            if (GetItemStackAt(slotIndex).IsEmpty)
            {
                //safe update
                OnRemovingItem(slotIndex);
                return null;
            }

            // Duplicate the stack (preserving specialized data if needed).

            var pivotPosition =  GetItemStackAt(slotIndex).PivotPosition;
            Debug.Log("Removing item from slot " + pivotPosition);
            
            ItemStack removed = CreateStack(pivotPosition, GetItemStackAt(slotIndex).ItemData, GetItemStackAt(slotIndex).Quantity, GetItemStackAt(slotIndex));
            
            OnRemovingItem(slotIndex);
            
            return removed;
        }
        
        /**
         * 1. Clear the item connections of the stack item
         * 2. Clear the item stack
         */
        private void OnRemovingItem(int slotIndex)
        {
            var itemStack = GetItemStackAt(slotIndex);
            //clear the item connections of the stack item
            foreach(var pos in itemStack.ItemPositions)
            {
                var sIndex = GridToSlotIndex(pos.x, pos.y);
                //remove information about the item in the slot
                slots[sIndex].Clear();
                OnInventorySlotChangedEvent(sIndex);
            }
            _itemStacks.Remove(itemStack);
        }

        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            if(CanFitIn(SlotIndexToGrid(slotIndex), itemStack.ItemData.ItemShape, out List<Vector2Int> requiredSlots))
            {
                // Actually place the item
                ItemStack placedStack = CreateItemStackAtLocation(requiredSlots, slotIndex, itemStack.ItemData, itemStack.Quantity, itemStack);
                _itemStacks.Add(placedStack);
                OnInventorySlotChangedEvent(slotIndex);
            }
        }
        
        // ----------------------------------------------
        // Remove item from the inventory by quantity
        // ----------------------------------------------
        protected virtual bool RemoveItem(ItemStack itemStack, int quantity = 1)
        {
            // if (itemStack == null || itemStack.IsEmpty || quantity <= 0)
            // {
            //     Debug.LogWarning("Cannot remove null or empty stack, or invalid quantity.");
            //     return false;
            // }
            //
            // int quantityToRemove = quantity;
            // for (int i = 0; i < Capacity; i++)
            // {
            //     var slot = slots[i];
            //     if (!slot.IsEmpty && slot.ItemData == itemStack.ItemData)
            //     {
            //         int amountToRemove = Math.Min(quantityToRemove, slot.Quantity);
            //         slot.Quantity -= amountToRemove;    // reduces the stack in this slot
            //         quantityToRemove -= amountToRemove;
            //
            //         if (slot.Quantity <= 0)
            //         {
            //             OnItemUsedUp(i);
            //             slot.Clear();
            //             _itemStacks[i] = new ItemStack();
            //         }
            //         OnInventorySlotChanged?.Invoke(i);
            //
            //         if (quantityToRemove <= 0)
            //         {
            //             return true; // done
            //         }
            //     }
            // }
            //
            // Debug.Log("Not enough items to remove.");
            return false;
        }
        
        
        // ----------------------------------------------
        // Remove item from a specific slot
        // ----------------------------------------------
        protected bool RemoveItemFromSlot(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            var itemStack = GetItemStackAt(slotIndex);
            if (itemStack.IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return false;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove.");
                return false;
            }

            if (itemStack.Quantity >= quantity)
            {
                itemStack.Quantity -= quantity;

                if (itemStack.Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    OnRemovingItem(slotIndex);
                    itemStack = new ItemStack();
                }
                OnInventorySlotChangedEvent(slotIndex);
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
            // Subclasses can override to handle item fully used (e.g. durability 0).
        }
        
        public abstract void LeftClickItem(int slotIndex);
        
        protected ItemStack CreateStack(Vector2Int pivotPosition, ItemData itemData, int quantity, ItemStack template)
        {
            var cItem = itemData as ContainerItem;
            var cStack = template as ContainerItemStack;

            Debug.Log("Creating stack with " + itemData.ItemName + " at " + pivotPosition + " with quantity " + quantity);
            
            if (cItem && cStack != null)
            {
                // Preserve container data from cStack
                return new ContainerItemStack(pivotPosition, cItem, quantity, cStack.AssociatedContainer);
            }
            else if (cItem)
            {
                // itemData is ContainerItem, template is not
                return new ContainerItemStack(pivotPosition, cItem, quantity, new PlayerContainer(null, cItem.width, cItem.height));
            }
            else if (cStack != null)
            {
                // Template is ContainerItemStack but itemData not ContainerItem
                Debug.LogWarning("Template was ContainerItemStack but itemData is not ContainerItem. Using normal ItemStack fallback.");
                return new ItemStack(pivotPosition, itemData, quantity);
            }
            else
            {
                // Normal item
                return new ItemStack(pivotPosition, itemData, quantity);
            }
        }
        
        public ItemStack CreateItemStackAtLocation(List<Vector2Int> requiredSlots, int slotIndex, ItemData itemData, int quantity, ItemStack template)
        {
            if (requiredSlots == null || requiredSlots.Count == 0)
            {
                throw new ArgumentException("Invalid required slots.");
            }
            
            // Check availability
            ItemStack newStack = CreateStack(SlotIndexToGrid(slotIndex), itemData, quantity, template);

            newStack.ItemPositions = requiredSlots;
            // All required slots free, create the new stack

            // Fill each required slot
            foreach (var slotPos in requiredSlots)
            {
                var sIndex = GridToSlotIndex(slotPos.x, slotPos.y);
                slots[sIndex].ItemStack = newStack;
                OnInventorySlotChangedEvent(sIndex);
            }
            return newStack;
        }
        
        
        
        
        
        
        
        
        
        
        // ----------------------------------------------
        // Coord helpers
        // ----------------------------------------------
        public int GridToSlotIndex(int x, int y)
        {
            return x * _height + y;
        }

        public Vector2Int SlotIndexToGrid(int slotIndex)
        {
            int gx = slotIndex / _height;
            int gy = slotIndex % _height;
            return new Vector2Int(gx, gy);
        }

        private bool IsInRange(int x, int y)
        {
            return (x >= 0 && x < _width && y >= 0 && y < _height);
        }
    }
}
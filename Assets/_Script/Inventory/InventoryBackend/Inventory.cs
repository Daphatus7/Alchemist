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
        protected readonly InventorySlot[] Slots;
        
        public int SlotCount => Slots.Length;
        
        public ItemStack GetItemStackAt(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }
            if(Slots[index] == null)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }
            return Slots[index] == null ? null : Slots[index].ItemStack;
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

            Slots = new InventorySlot[Capacity];
            _itemStacks = new List<ItemStack>();

            // Initialize slots
            for (int i = 0; i < Capacity; i++)
            {
                Slots[i] = new InventorySlot();
            }
        }

        /// <summary>
        /// Load an inventory with items (restoring from save, etc.)
        /// </summary>
        public Inventory(int height, int width, ItemStack[] itemStack)
        {
            _height = height;
            _width = width;

            Slots = new InventorySlot[Capacity];
            _itemStacks = new List<ItemStack>();

            // Initialize slots
            for (int i = 0; i < Capacity; i++)
            {
                Slots[i] = new InventorySlot();
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
        
        public event Action OnItemStackChanged;

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
            foreach(var item in _itemStacks)
            {
                var existingStack = item;
                if (!existingStack.IsEmpty &&
                    existingStack.ItemData.Equals(itemStackToAdd.ItemData) &&
                    existingStack.Quantity < existingStack.ItemData.MaxStackSize)
                {
                    int oldQuantity = itemStackToAdd.Quantity;
                    int remaining   = existingStack.TryAdd(itemStackToAdd);
                    itemStackToAdd.Quantity = remaining;

                    if (remaining < oldQuantity)
                    {
                        // Some merging happened
                        OnInventorySlotChangedEvent(GridToSlotIndex(existingStack.PivotPosition.x, existingStack.PivotPosition.y));
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
                if (CanFitIn(itemStackToAdd.ItemData.ItemShape.ProjectedPositions(SlotIndexToGrid(i))))
                {
                    // Decide how many to place
                    int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);

                    // Actually place the item
                    ItemStack placedStack = CreateItemStackAtLocation(itemStackToAdd.ItemData.ItemShape.ProjectedPositions(SlotIndexToGrid(i)), i, itemStackToAdd.ItemData, toAdd, itemStackToAdd);
                    if (placedStack != null)
                    {
                        // Adjust leftover
                        itemStackToAdd.Quantity -= toAdd;
                        _itemStacks.Add(placedStack);
                        OnOnItemStackChanged();
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
        private bool CanFitIn(List<Vector2Int> projectedPositions)
        {
            
            foreach (var pos in projectedPositions)
            {
                int gx = pos.x;
                int gy = pos.y;

                // Check bounds
                if (gx < 0 || gx >= _width || gy < 0 || gy >= _height)
                {
                    return false;
                }

                int finalIndex = GridToSlotIndex(gx, gy);
                if (!Slots[finalIndex].IsEmpty)
                {
                    return false;
                }
            }
            
            return true;
        }


        public bool CanFitItem(List<Vector2Int> projectedPositions)
        {
            
            if (projectedPositions == null || projectedPositions.Count == 0)
            {
                Debug.Log("Invalid item stack to compare.");
                return false;
            }
            
//            Debug.Log("Cp " + SlotIndexToGrid(mySlot));
            
            return CanFitIn(projectedPositions);    
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
            //Debug.Log("Removing item from slot " + pivotPosition);
            
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
                Slots[sIndex].Clear();
                OnInventorySlotChangedEvent(sIndex);
            }
            _itemStacks.Remove(itemStack);
            OnOnItemStackChanged();
        }

        public void AddItemToEmptySlot(ItemStack itemStack, int selectedSlotIndex)
        {
            var pos = SlotIndexToGrid(selectedSlotIndex);
            var projectedPositions = itemStack.ItemData.ItemShape.ProjectedPositions(pos);
            if(CanFitIn(projectedPositions))
            {
                // Actually place the item
                ItemStack placedStack = CreateItemStackAtLocation(projectedPositions, selectedSlotIndex, itemStack.ItemData, itemStack.Quantity, itemStack);
                _itemStacks.Add(placedStack);
                OnOnItemStackChanged();
                OnInventorySlotChangedEvent(selectedSlotIndex);
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

            //Debug.Log("Creating stack with " + itemData.ItemName + " at " + pivotPosition + " with quantity " + quantity);
            
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
        
        public ItemStack CreateItemStackAtLocation(List<Vector2Int> projectedLocations, int slotIndex, ItemData itemData, int quantity, ItemStack template)
        {
            if (projectedLocations == null || projectedLocations.Count == 0)
            {
                Debug.LogWarning("Invalid item stack to place.");
                return null;
            }
            
            // Check availability
            ItemStack newStack = CreateStack(SlotIndexToGrid(slotIndex), itemData, quantity, template);

            newStack.ItemPositions = projectedLocations;
            // All required slots free, create the new stack

            // Fill each required slot
            foreach (var slotPos in projectedLocations)
            {
                var sIndex = GridToSlotIndex(slotPos.x, slotPos.y);
                Slots[sIndex].ItemStack = newStack;
                OnInventorySlotChangedEvent(sIndex);
            }
            return newStack;
        }
        
        public int GetItemsCountAtPositions(int pivotIndex
            , List<Vector2Int> projectedPositions, out int overlapIndex)
        {
            overlapIndex = -1;

            if (pivotIndex < 0 || pivotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return 0;
            }
            
            var itemPivot = SlotIndexToGrid(pivotIndex);
            
            var foundItem = new Dictionary<ItemStack, int>();
            
            // check each slot by adding the offset to the pivot
            foreach(var offset in projectedPositions)
            {
                var offsetPos = itemPivot + offset;
                var sIndex = GridToSlotIndex(offsetPos.x, offsetPos.y);
                if(sIndex < 0 || sIndex >= Capacity)
                {
                    return Int32.MaxValue;
                }
                var slot = Slots[sIndex];
                if (!slot.IsEmpty)
                {
                    overlapIndex = sIndex; //Any overlap index, but only if the count is 1 will be considered
                    if (!foundItem.TryAdd(Slots[sIndex].ItemStack, 1))
                    {
                        foundItem[Slots[sIndex].ItemStack]++;
                    }
                }
            }
            return foundItem.Count;
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

        protected void OnOnItemStackChanged()
        {
            OnItemStackChanged?.Invoke();
        }
    }

 
}
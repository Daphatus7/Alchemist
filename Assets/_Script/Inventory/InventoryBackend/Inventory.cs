using System;
using System.Collections.Generic;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEditor.PackageManager;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
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
        private readonly List<ItemStack> _itemStacks; public List<ItemStack> ItemStacks => _itemStacks;

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
        public void OnInventorySlotChangedEvent(List<Vector2Int> positions)
        {
            foreach (var pos in positions)
            {
                OnInventorySlotChanged?.Invoke(GridToSlotIndex(pos.x, pos.y));
            }
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
                        OnInventorySlotChangedEvent(existingStack.ItemPositions);
                        
                        int mergedQuantity = oldQuantity - remaining;
                        InventoryStatus.UpdateInventoryStatus(existingStack.ItemData.itemID, mergedQuantity);
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
                var projectedPositions = itemStackToAdd.ItemData.ItemShape.ProjectedPositions(SlotIndexToGrid(i));
                if (CanFitIn(projectedPositions))
                {
                    // Decide how many to place
                    int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);

                    // Actually place the item
                    ItemStack placedStack = CreateItemStackAtLocation(projectedPositions, toAdd, itemStackToAdd);
                    if (placedStack != null)
                    {
                        // Adjust leftover
                        itemStackToAdd.Quantity -= toAdd;
                        _itemStacks.Add(placedStack);
                        OnOnItemStackChanged();
                        InventoryStatus.UpdateInventoryStatus(placedStack.ItemData.itemID, toAdd);

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
            if (slotIndex < 0 || slotIndex >= Capacity) return null;
            
            var itemAtSlot = GetItemStackAt(slotIndex);
            if (itemAtSlot == null)
            {
                return null;
            }
            if (itemAtSlot.IsEmpty)
            {
                //safe update
                OnRemovingItem(slotIndex);
                return null;
            }

            
            ItemStack removed = CreateStack(itemAtSlot.ItemPositions, itemAtSlot.Quantity, itemAtSlot);
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
            if(itemStack == null) return;  // Remove the check for IsEmpty
            // Clear the inventory status using the remaining quantity (if any)
            InventoryStatus.UpdateInventoryStatus(itemStack.ItemData.itemID, -itemStack.Quantity);
            foreach(var pos in itemStack.ItemPositions)
            {
                var sIndex = GridToSlotIndex(pos.x, pos.y);
                Slots[sIndex].Clear();
            }
            OnInventorySlotChangedEvent(itemStack.ItemPositions);
            _itemStacks.Remove(itemStack);
            OnOnItemStackChanged();
        }


        public void AddItemToEmptySlot(ItemStack itemStack, List<Vector2Int> projectedPositions)
        {
            ItemStack placedStack = CreateItemStackAtLocation(projectedPositions, itemStack.Quantity, itemStack);
            _itemStacks.Add(placedStack);
            OnOnItemStackChanged();
            OnInventorySlotChangedEvent(projectedPositions);
            InventoryStatus.UpdateInventoryStatus(placedStack.ItemData.itemID, placedStack.Quantity);
        }
        
        // ----------------------------------------------
        // Remove item from the inventory by quantity
        // ----------------------------------------------
        public virtual bool RemoveItemById(string itemId, int quantity = 1)
        {
            // Validate input
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("Invalid itemId provided.");
                return false;
            }
            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove: " + quantity);
                return false;
            }

            // First, compute the total available amount for the specified itemId.
            int totalAvailable = 0;
            foreach (var stack in _itemStacks)
            {
                if (stack != null && !stack.IsEmpty && stack.ItemData.ItemID.Equals(itemId))
                {
                    totalAvailable += stack.Quantity;
                }
            }

            // If there aren’t enough items available, then nothing will be removed.
            if (totalAvailable < quantity)
            {
                Debug.Log($"Not enough items with id {itemId} to remove. Requested: {quantity}, available: {totalAvailable}");
                return false;
            }

            // Remove the required quantity from stacks.
            int remaining = quantity;
            // Iterate over a copy so that if a whole stack is removed (which clears it from _itemStacks)
            // the iteration is not affected.
            List<ItemStack> stacksCopy = new List<ItemStack>(_itemStacks);
            foreach (var stack in stacksCopy)
            {
                if (remaining <= 0)
                    break;

                if (stack != null && !stack.IsEmpty && stack.ItemData.ItemID.Equals(itemId))
                {
                    if (stack.Quantity <= remaining)
                    {
                        // If this stack has less than or equal to the remaining quantity,
                        // remove the whole stack.
                        remaining -= stack.Quantity;
                        // Pick one of the positions to trigger the removal. OnRemovingItem will clear
                        // all slots associated with the item and update InventoryStatus.
                        int slotIndex = GridToSlotIndex(stack.ItemPositions[0].x, stack.ItemPositions[0].y);
                        OnRemovingItem(slotIndex);
                    }
                    else
                    {
                        // If the current stack has more than we need, just subtract the required amount.
                        stack.Quantity -= remaining;
                        InventoryStatus.UpdateInventoryStatus(itemId, -remaining);
                        OnInventorySlotChangedEvent(stack.ItemPositions);
                        // Notify that the item stacks have changed.
                        OnOnItemStackChanged();
                        remaining = 0;
                    }
                }
            }
            return true;
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

                InventoryStatus.UpdateInventoryStatus(itemStack.ItemData.ItemID, -quantity);

                if (itemStack.Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    OnRemovingItem(slotIndex);
                    itemStack = new ItemStack();
                }
                OnInventorySlotChangedEvent(itemStack.ItemPositions);
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
            Debug.Log("Item used up.");
        }
        
        public abstract void LeftClickItem(int slotIndex);
        
        protected ItemStack CreateStack(List<Vector2Int> projectedLocations, int quantity, ItemStack item)
        {
            var cItem = item.ItemData as ContainerItem;
            var cStack = item as ContainerItemStack;

            //Debug.Log("Creating stack with " + itemData.ItemName + " at " + pivotPosition + " with quantity " + quantity);
            
            if (cItem && cStack != null)
            {
                // Preserve container data from cStack
                return new ContainerItemStack(cItem, quantity, cStack.AssociatedContainer);
            }
            else if (cItem)
            {
                // itemData is ContainerItem, template is not
                return new ContainerItemStack(cItem, quantity, new PlayerContainer(null, cItem.width, cItem.height));
            }
            else if (cStack != null)
            {
                // Template is ContainerItemStack but itemData not ContainerItem
                Debug.LogWarning("Template was ContainerItemStack but itemData is not ContainerItem. Using normal ItemStack fallback.");
                return new ItemStack(projectedLocations, item, quantity);
            }
            else
            {
                // Normal item
                return new ItemStack(projectedLocations, item, quantity);
            }
        }
        
        public ItemStack CreateItemStackAtLocation(List<Vector2Int> projectedLocations, int quantity, ItemStack itemStack)
        {
            if (projectedLocations == null || projectedLocations.Count == 0)
            {
                Debug.LogWarning("Invalid item stack to place.");
                return null;
            }
            
            // Check availability
            ItemStack newStack = CreateStack(projectedLocations, quantity, itemStack);

            newStack.ItemPositions = projectedLocations;
            // All required slots free, create the new stack

            // Fill each required slot
            foreach (var slotPos in projectedLocations)
            {
                var sIndex = GridToSlotIndex(slotPos.x, slotPos.y);
                Slots[sIndex].ItemStack = newStack;
            }
            
            OnInventorySlotChangedEvent(projectedLocations);
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

        private void OnOnItemStackChanged()
        {
            OnItemStackChanged?.Invoke();
        }

        #region Inventory Status

        private InventoryStatus InventoryStatus { get; } = new InventoryStatus();

        public void SubscribeToInventoryStatus(Action<string, int> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            InventoryStatus.OnInventoryStatusChanged += action;
        }
        
        public void UnsubscribeToInventoryStatus(Action<string, int> action)
        {
            InventoryStatus.OnInventoryStatusChanged -= action;
        }

        #endregion
    }
       
    /// <summary>
    /// string : item id
    /// int : quantity
    /// </summary>
    public sealed class InventoryStatus
    {
        private Dictionary<string, int> Status { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> GetStatus => Status;
        
        public event Action<string, int> OnInventoryStatusChanged;

        internal void UpdateInventoryStatus(string itemID, int quantityChange)
        {
            // If adding a new itemID that doesn't exist yet
            if (!Status.ContainsKey(itemID) && quantityChange > 0)
            {
                Status[itemID] = quantityChange;
                OnOnInventoryStatusChanged(itemID, quantityChange);
                return;
            }

            // If updating an existing itemID
            if (Status.ContainsKey(itemID))
            {
                Status[itemID] += quantityChange;

                // Remove if it goes to zero or negative
                if (Status[itemID] <= 0)
                {
                    Status.Remove(itemID);
                    OnOnInventoryStatusChanged(itemID, 0); // Send 0 explicitly instead of Status[itemID]
                    return; // Early return to prevent invalid Status[itemID] lookup
                }
            }

            // Send update only if the item still exists
            if (Status.ContainsKey(itemID))
            {
                OnOnInventoryStatusChanged(itemID, Status[itemID]);
            }
            else
            {
                OnOnInventoryStatusChanged(itemID, 0); // Ensure zero is reported if removed
            }
        }


        private void PrintStatus()
        {
            Debug.Log("Current Inventory Status: ");
            foreach (var pair in Status)
            {
                Debug.Log(pair.Key + " : " + pair.Value);
            }
            Debug.Log("----------End of Inventory Status----------");
        }

        private void OnOnInventoryStatusChanged(string itemID, int count)
        {
            OnInventoryStatusChanged?.Invoke(itemID, count);
        }
    }

}
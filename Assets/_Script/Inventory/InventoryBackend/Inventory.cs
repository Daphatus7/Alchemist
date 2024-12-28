using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Inventory.SlotFrontend;
using Unity.VisualScripting;

namespace _Script.Inventory.InventoryBackend
{
    /**
     * The individual slot in the inventory.
     * Each slot holds a reference to an ItemStack (or null if empty).
     */
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
        private readonly int _height;
        private readonly int _width;

        public int Capacity => _height * _width;

        /**
         * The shape-based slots in the inventory.
         * Each slot is an InventorySlot that may or may not contain an ItemStack.
         */
        protected readonly InventorySlot[] slots; public InventorySlot[] Slots => slots;
        
        public ItemStack GetItemStackAt(int index)
        {
            return slots[index].ItemStack;
        }

        /**
         * A parallel array used for partial stack merges (linear approach).
         * Some designs unify this with 'slots', but here we keep them separate
         * for demonstration: e.g., `_itemStacks[i]` matches `slots[i].ItemStack`.
         */
        protected readonly ItemStack[] _itemStacks;

        public ItemStack[] ItemStacks => _itemStacks;

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
            _itemStacks = new ItemStack[Capacity];

            for (int i = 0; i < Capacity; i++)
            {
                slots[i] = new InventorySlot();
                _itemStacks[i] = new ItemStack(); // empty
            }
        }

        /// <summary>
        /// Load an inventory with items (restoring from save, etc.)
        /// </summary>
        public Inventory(int height, int width, ItemStack[] items)
        {
            _height = height;
            _width = width;

            slots = new InventorySlot[Capacity];
            _itemStacks = new ItemStack[Capacity];

            for (int i = 0; i < Capacity; i++)
            {
                slots[i] = new InventorySlot();

                if (items != null && i < items.Length && items[i] != null && !items[i].IsEmpty)
                {
                    // Use CreateStack to preserve special data if it's a specialized stack
                    _itemStacks[i] = CreateStack(items[i].ItemData, items[i].Quantity, items[i]);
                    slots[i].ItemStack = _itemStacks[i];
                }
                else
                {
                    // empty
                    _itemStacks[i] = new ItemStack();
                }
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
            for (int i = 0; i < _itemStacks.Length; i++)
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
            for (int i = 0; i < Capacity; i++)
            {
                // If the top-left slot is empty, check if shape can fit
                if (slots[i].IsEmpty)
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
        private bool CanFitIn(Vector2Int pos, ItemShape shape, out List<Vector2Int> requiredSlots)
        {
            requiredSlots = new List<Vector2Int>();

            if (shape == null || shape.Positions == null || shape.Positions.Count == 0)
            {
                throw new ArgumentException("Invalid shape data.");
            }

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

        // ----------------------------------------------
        // AddItemToEmptySlot (simple 1x1)
        // ----------------------------------------------
        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            if (slots[slotIndex].IsEmpty)
            {
                slots[slotIndex].ItemStack = CreateStack(itemStack.ItemData, itemStack.Quantity, itemStack);
                _itemStacks[slotIndex] = slots[slotIndex].ItemStack; // keep in sync
                OnInventorySlotChanged?.Invoke(slotIndex);
            }
            else
            {
                Debug.LogWarning("Target slot is not empty, cannot add item directly.");
            }
        }

        // ----------------------------------------------
        // Lower-level add to an existing slot (partial merges)
        // ----------------------------------------------
        protected bool AddItemToSlot(ItemStack itemStackToAdd, int slotIndex)
        {
            if (itemStackToAdd == null || itemStackToAdd.IsEmpty)
            {
                Debug.LogWarning("ItemData is null or stack is empty.");
                return false;
            }

            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            var slot = slots[slotIndex];
            if (!slot.IsEmpty && slot.ItemData == itemStackToAdd.ItemData && slot.Quantity < itemStackToAdd.ItemData.MaxStackSize)
            {
                int remaining = slot.TryAdd(itemStackToAdd);
                _itemStacks[slotIndex] = slot.ItemStack; // keep in sync
                
                OnInventorySlotChangedEvent(slotIndex);
                return (remaining < itemStackToAdd.Quantity);
            }

            if (slot.IsEmpty)
            {
                int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);
                slot.ItemStack = CreateStack(itemStackToAdd.ItemData, toAdd, itemStackToAdd);
                _itemStacks[slotIndex] = slot.ItemStack;
                
                OnInventorySlotChangedEvent(slotIndex);
                itemStackToAdd.Quantity -= toAdd;
                return toAdd > 0;
            }

            Debug.Log("Slot is full or not compatible with the item type.");
            return false;
        }

        // ----------------------------------------------
        // Remove entire stack from a slot
        // ----------------------------------------------
        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            //invalid slot index
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }

            // Slot is empty
            if (slots[slotIndex].IsEmpty)
            {
                //safe update
                OnInventorySlotChangedEvent(slotIndex);
                return null;
            }

            // Duplicate the stack (preserving specialized data if needed).
            ItemStack removed = CreateStack(slots[slotIndex].ItemData, slots[slotIndex].Quantity, slots[slotIndex].ItemStack);
            
            foreach (var pos in removed.ItemPositions)
            {
                var sIndex = GridToSlotIndex(pos.x, pos.y);
                slots[sIndex].Clear();
                OnInventorySlotChangedEvent(sIndex);
            }
            
            _itemStacks[slotIndex] = null;

            OnInventorySlotChangedEvent(slotIndex);
            return removed;
        }
        
        private void OnRemovingItem(int slotIndex)
        {
            //1 remove the connection between the item and the container
        }
        

        // ----------------------------------------------
        // Remove item from the inventory by quantity
        // ----------------------------------------------
        protected virtual bool RemoveItem(ItemStack itemStack, int quantity = 1)
        {
            if (itemStack == null || itemStack.IsEmpty || quantity <= 0)
            {
                Debug.LogWarning("Cannot remove null or empty stack, or invalid quantity.");
                return false;
            }

            int quantityToRemove = quantity;
            for (int i = 0; i < Capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.ItemData == itemStack.ItemData)
                {
                    int amountToRemove = Math.Min(quantityToRemove, slot.Quantity);
                    slot.Quantity -= amountToRemove;    // reduces the stack in this slot
                    quantityToRemove -= amountToRemove;

                    if (slot.Quantity <= 0)
                    {
                        OnItemUsedUp(i);
                        slot.Clear();
                        _itemStacks[i] = new ItemStack();
                    }
                    OnInventorySlotChanged?.Invoke(i);

                    if (quantityToRemove <= 0)
                    {
                        return true; // done
                    }
                }
            }

            Debug.Log("Not enough items to remove.");
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

            var slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return false;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove.");
                return false;
            }

            if (slot.Quantity >= quantity)
            {
                slot.Quantity -= quantity;

                if (slot.Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    slot.Clear();
                    _itemStacks[slotIndex] = new ItemStack();
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
            // Subclasses can override to handle item fully used (e.g. durability 0).
        }

        // ----------------------------------------------
        // Abstract: handle a left-click on a slot
        // ----------------------------------------------
        public abstract void LeftClickItem(int slotIndex);

        // ----------------------------------------------
        // CreateStack: decides ContainerItemStack vs normal
        // ----------------------------------------------
        protected ItemStack CreateStack(ItemData itemData, int quantity, ItemStack template)
        {
            var cItem = itemData as ContainerItem;
            var cStack = template as ContainerItemStack;

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
                return new ItemStack(itemData, quantity);
            }
            else
            {
                // Normal item
                return new ItemStack(itemData, quantity);
            }
        }

        // ----------------------------------------------
        // Shape-based item creation
        // ----------------------------------------------
        public ItemStack CreateItemStackAtLocation(List<Vector2Int> requiredSlots, int slotIndex, ItemData itemData, int quantity, ItemStack template)
        {
            // (x,y) of the top-left corner
            var position = SlotIndexToGrid(slotIndex);

            // Get shape offsets
            var shapeOffsets = itemData.ItemShape?.Positions;
            if (shapeOffsets == null || shapeOffsets.Count == 0)
            {
                // 1×1 fallback
                shapeOffsets = new List<Vector2Int> { Vector2Int.zero };
            }

            // Check availability
            ItemStack newStack = CreateStack(itemData, quantity, template);

            newStack.ItemPositions = requiredSlots;
            // All required slots free, create the new stack

            // Fill each required slot
            foreach (var slotPos in requiredSlots)
            {
                var sIndex = GridToSlotIndex(slotPos.x, slotPos.y);
                slots[sIndex].ItemStack = newStack;
                _itemStacks[sIndex] = newStack;  // Keep parallel array in sync
                OnInventorySlotChangedEvent(sIndex);
            }

            return newStack;
        }

        // ----------------------------------------------
        // Coord helpers
        // ----------------------------------------------
        private int GridToSlotIndex(int x, int y)
        {
            return y * _width + x;
        }

        private Vector2Int SlotIndexToGrid(int slotIndex)
        {
            int gx = slotIndex % _width;
            int gy = slotIndex / _width;
            return new Vector2Int(gx, gy);
        }

        private bool IsInRange(int x, int y)
        {
            return (x >= 0 && x < _width && y >= 0 && y < _height);
        }
    }
}
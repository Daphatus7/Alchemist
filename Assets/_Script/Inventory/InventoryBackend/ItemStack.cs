using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    /**
     * The entity that represents an item in the inventory
     */
    [System.Serializable]
    public class ItemStack
    {
        public ItemData ItemData { get; private set; }
        public int Quantity { get; set; }
        
        /**
         * all the positions of the item in the inventory
         */
        public List<Vector2Int> ItemPositions { get; set; } = new List<Vector2Int>();
        
        
        public bool IsEmpty => ItemData == null || Quantity <= 0;

        public ItemStack(ItemStack stack)
        {
            if (stack == null || stack.ItemData == null || stack.Quantity <= 0)
            {
                Clear();
            }
            else
            {
                ItemData = stack.ItemData;
                Quantity = Mathf.Clamp(stack.Quantity, 0, stack.ItemData.MaxStackSize);
            }
        }

        public ItemStack()
        {
            Clear();
        }

        public ItemStack(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Clear();
                return;
            }

            ItemData = itemData;
            Quantity = Mathf.Clamp(quantity, 0, itemData.MaxStackSize);
        }

        public void Clear()
        {
            ItemData = null;
            Quantity = 0;
        }

        /// <summary>
        /// Attempts to add another stack of the same type to this stack.
        /// Returns the remaining quantity of the other stack after adding.
        /// If the entire other stack can be added, returns 0.
        /// </summary>
        public int TryAdd(ItemStack other)
        {
            if (other == null || other.IsEmpty || other.ItemData != this.ItemData)
                return other?.Quantity ?? 0;

            int space = ItemData.MaxStackSize - Quantity;
            int toAdd = Mathf.Min(space, other.Quantity);
            Quantity += toAdd;
            return other.Quantity - toAdd;
        }

        /// <summary>
        /// Splits a specified quantity from this stack and returns it as a new stack.
        /// If there isn’t enough quantity, it returns as many as possible.
        /// After splitting, this stack’s quantity decreases accordingly.
        /// </summary>
        public ItemStack Split(int count)
        {
            if (IsEmpty || count <= 0)
                return new ItemStack(); // Returns an empty stack

            int actualCount = Mathf.Min(count, Quantity);
            Quantity -= actualCount;
            return new ItemStack(ItemData, actualCount);
        }
    }
}
using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
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
        
        private bool _rotated = false; public bool IsRotated => _rotated;

        // public ItemStack(ItemStack stack)
        // {
        //     if (stack == null || stack.ItemData == null || stack.Quantity <= 0)
        //     {
        //         Clear();
        //     }
        //     else
        //     {
        //         ItemData = Object.Instantiate(stack.ItemData);
        //         _rotated = stack.IsRotated;
        //         Quantity = Mathf.Clamp(stack.Quantity, 0, stack.ItemData.MaxStackSize);
        //     }
        // }
        
        public bool ToggleRotate(Vector2Int rotatePivot)
        {
            // If there's no valid ItemData, do nothing.
            if (!ItemData) return false;

            // Prepare a new list to hold the rotated positions.
            var rotatedPositions = new List<Vector2Int>(ItemPositions.Count);

            foreach (Vector2Int pos in ItemPositions)
            {
                // Translate current position into pivot-relative coordinates.
                Vector2Int relative = pos - rotatePivot;
        
                // If not rotated yet, rotate clockwise 90°: (x, y) -> (-y, x)
                // If already rotated, rotate counterclockwise 90°: (x, y) -> (y, -x)
                Vector2Int newRelative = _rotated
                    ? new Vector2Int(relative.y, -relative.x)     // counterclockwise
                    : new Vector2Int(-relative.y, relative.x);    // clockwise
        
                // Translate back by adding the pivot.
                rotatedPositions.Add(rotatePivot + newRelative);
            }

            // Update the item’s positions with the newly rotated ones.
            ItemPositions = rotatedPositions;
            _rotated = !_rotated;
            // Flip the IsRotated state on the ItemShape and return the new value.
            return _rotated;
        }
        
        public ItemStack()
        {
            Clear();
        }

        public ItemStack(ItemData itemData, int quantity = 1)
        {
            if (!itemData)
            {
                Clear();
                return;
            }
            
            ItemData = Object.Instantiate(itemData);
            ItemData.ItemShape = new ItemShape(itemData.ItemShape);
            
            Quantity = Mathf.Clamp(quantity, 0, itemData.MaxStackSize);
        }
        
        public ItemStack(List<Vector2Int> projectedPositions, ItemStack item, int quantity = 1)
        {
            if (item == null || !item.ItemData || quantity <= 0)
            {
                Clear();
                return;
            }
            
            ItemPositions = new List<Vector2Int>(projectedPositions);
            ItemData = Object.Instantiate(item.ItemData);
            ItemData.ItemShape = new ItemShape(item.ItemData.ItemShape);
            _rotated = item.IsRotated;
            Quantity = Mathf.Clamp(quantity, 0, item.ItemData.MaxStackSize);
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
            if (other == null || other.IsEmpty || !ItemData.Equals(other.ItemData))
                return other?.Quantity ?? 0;
            
            Debug.Log("Adding " + other.Quantity + " of " + ItemData.ItemName + " to " + Quantity);
            int space = ItemData.MaxStackSize - Quantity;
            int toAdd = Mathf.Min(space, other.Quantity);
            Quantity += toAdd;
            return other.Quantity - toAdd;
        }
    }
    

}
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
        public Vector2Int PivotPosition { get; set; }
        
        public bool IsEmpty => ItemData == null || Quantity <= 0;

        public ItemStack(ItemStack stack)
        {
            if (stack == null || stack.ItemData == null || stack.Quantity <= 0)
            {
                Clear();
            }
            else
            {
                ItemData = Object.Instantiate(stack.ItemData);
                Quantity = Mathf.Clamp(stack.Quantity, 0, stack.ItemData.MaxStackSize);
            }
        }
        
        public ItemStack()
        {
            Clear();
        }

        public ItemStack(Vector2Int pivotPosition, ItemData itemData, int quantity = 1)
        {
            if (!itemData)
            {
                Clear();
                return;
            }

            PivotPosition = pivotPosition;

            Debug.Log("Creating item stack with rotation---- " + itemData.ItemShape.IsRotated);
            foreach (var pos in itemData.ItemShape.Positions)
            {
                Debug.Log("Position: " + pos);
            }
            
            ItemData = Object.Instantiate(itemData);
            
            ItemData.ItemShape = new ItemShape(itemData.ItemShape);
            
            Debug.Log("Creating item stack with rotation---- " + ItemData.ItemShape.IsRotated);
            foreach (var pos in ItemData.ItemShape.Positions)
            {
                Debug.Log("Position: " + pos);
            }
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
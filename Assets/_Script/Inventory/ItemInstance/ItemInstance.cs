using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Items.Helper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Script.Inventory.ItemInstance
{
    /**
     * The entity that represents an item in the inventory
     */
    [Serializable]
    public class ItemInstance
    {
        //可能不需要保存ItemData, 可以直接通过ItemData的ID来获取
        protected ItemData ItemData { get; set; }
        
        public string ItemID => ItemData.itemID;
        public string ItemDescription => ItemData.itemDescription;
        public string ItemTypeString => ItemData.ItemTypeString;

        #region Save required
        public int Quantity { get; set; }
        
        private bool _rotated = false; public bool IsRotated => _rotated;

        #endregion

        private List<Vector2Int> _itemPositions;
        
        
        /**
         * all the positions of the item in the inventory
         */
        public List<Vector2Int> ItemPositions
        {
            set
            {
                if (value == null)
                {
                    Debug.LogError("ItemPositions is null");
                }
                _itemPositions = value;
            }
            get
            {
                if (_itemPositions == null)
                {
                    Debug.LogError("ItemPositions was not set check");
                }
                return _itemPositions;
            }
        }
        
        [Obsolete("just remove it ")]
        public bool IsEmpty => ItemData == null || Quantity <= 0;
        
        
        /// <summary>
        /// RenderingPivot is the pivot point for rendering the item.
        /// Hardcoded solution
        /// </summary>
        public Vector2Int RenderingPivot => ItemPositions[ItemData.GetPivotIndex(_rotated)];
        /// <summary>
        /// Offset for rendering display of the item.
        /// Hardcoded solution
        /// </summary>
        public Vector3 RenderingOffset => ItemData.GetRenderingOffset(_rotated);
        public string ItemName => ItemData.ItemName;
        public int MaxStackSize => ItemData.MaxStackSize;
        public ItemShape ItemShape => ItemData.ItemShape;
        public Sprite ItemIcon => ItemData.itemIcon;
        public int Value => ItemData.Value;
        public Rarity Rarity => ItemData.rarity;

        public bool Use(PlayerCharacter player)
        {
            if (ItemData == null)
            {
                throw new Exception("ItemData is null ???!!!!");
            }
            return ItemData.Use(player);
        }
        
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
        
        public ItemInstance(ItemData itemData, bool rotated = false, int quantity = 1)
        {
            _rotated = rotated;
            ItemData = itemData; //copy reference instead of object
            Quantity = Mathf.Clamp(quantity, 0, quantity);
        }

        /// <summary>
        /// Attempts to add another stack of the same type to this stack.
        /// Returns the remaining quantity of the other stack after adding.
        /// If the entire other stack can be added, returns 0.
        /// </summary>
        public int TryAdd(ItemInstance other)
        {
            if (other == null || !ItemData.Equals(other.ItemData))
                return other?.Quantity ?? 0;
            
            //Debug.Log("Adding " + other.Quantity + " of " + ItemData.ItemName + " to " + Quantity);
            int space = ItemData.MaxStackSize - Quantity;
            int toAdd = Mathf.Min(space, other.Quantity);
            Quantity += toAdd;
            return other.Quantity - toAdd;
        }
        
        public bool Equals(ItemInstance other)
        {
            return other != null && ItemID == other.ItemID;
        }
        
        /// <summary>
        /// Create a new ItemInstance with the same ItemData, but with substracted quantity.
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public ItemInstance Split(int quantity)
        {
            if (quantity <= 0 || quantity >= Quantity)
            {
                Debug.LogError("Invalid quantity to split: " + quantity);
                return null;
            }
            
            Quantity -= quantity;
            return new ItemInstance(ItemData, _rotated, quantity);
        }
        
        /// <summary>
        /// Copy the ItemInstance with the same ItemData and quantity.
        /// </summary>
        /// <returns></returns>
        public ItemInstance Clone()
        {
            return new ItemInstance(ItemData, _rotated, Quantity);
        }
    }
    

}
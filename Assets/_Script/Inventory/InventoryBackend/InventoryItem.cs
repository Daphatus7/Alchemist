using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    [System.Serializable]
    public class InventoryItem
    {
        private ItemData _itemData; public ItemData ItemData => _itemData;

        public Sprite Icon => _itemData != null ? _itemData.ItemSprite : null;

        public string ItemName => _itemData != null ? _itemData.ItemName : null;

        private int quantity = 1;
        
        public int Quantity
        {
            get => quantity;
            set => quantity = Mathf.Max(0, value); // Ensure quantity is non-negative
        }

        // Constructor for copying an InventoryItem
        public InventoryItem(InventoryItem item)
        {
            _itemData = item.ItemData;
            quantity = item.Quantity;
        }

        public InventoryItem()
        {
            _itemData = null;
            quantity = 0;
        }

        // Constructor for creating an item with data and quantity
        public InventoryItem(ItemData itemData, int quantity = 1)
        {
            this._itemData = itemData;
            this.quantity = quantity;
        }

        public bool IsEmpty
        {
            get
            {
                return quantity == 0 || _itemData == null;
            }
        }

        public void Clear()
        {
            _itemData = null;
            quantity = 0;
        }
    }
}
using UnityEngine;

namespace _Script.Items
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public Sprite Icon => itemData != null ? itemData.icon : null;
        public string ItemName => itemData != null ? itemData.itemName : null;
        public int quantity;

        public InventoryItem(ItemData itemData, int quantity)
        {
            this.itemData = itemData;
            this.quantity = quantity;
        }
    }
}
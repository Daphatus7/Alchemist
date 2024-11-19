using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;
using Sirenix.OdinInspector; // Make sure Odin is imported

namespace _Script.Items
{
    [System.Serializable]
    public class InventoryItem
    {
        [ReadOnly, SerializeField] protected ItemData itemData; 
        public ItemData ItemData => itemData;

        // Use Odin's ReadOnly to display but prevent changes in the Inspector
        [ReadOnly, ShowInInspector]
        public Sprite Icon => itemData != null ? itemData.ItemIcon : null; // Referencing ItemIcon with the public getter

        // Similar for ItemName, using ShowInInspector to display
        [ReadOnly, ShowInInspector]
        protected string ItemName => itemData != null ? itemData.ItemName : null; 
        public string GetName() => ItemName;

        [SerializeField, ShowInInspector] 
        protected int quantity; 
        public int Quantity
        {
            get => quantity;
            set => quantity = value;
        }

        // Constructor
        public InventoryItem(InventoryItem item)
        {
            itemData = item.ItemData;
            this.quantity = quantity;
        }
        public InventoryItem(ItemData itemData, int quantity = 1)
        {
            this.itemData = itemData;
            this.quantity = quantity;
        }
    }
}
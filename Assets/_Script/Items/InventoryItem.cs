using UnityEngine;

namespace _Script.Items
{
    [System.Serializable]
    public class InventoryItem
    {
        [SerializeField] protected ItemData itemData; public ItemData ItemData => itemData;
        [SerializeField] public Sprite Icon => itemData != null ? itemData.icon : null; public Sprite GetIcon() => Icon;
        [SerializeField] protected string ItemName => itemData != null ? itemData.itemName : null; public string GetName() => ItemName;
        [SerializeField] protected int quantity; public int Quantity
        {
            get => quantity;
            set => quantity = value;
        }

        public InventoryItem(ItemData itemData, int quantity)
        {
            this.itemData = itemData;
            this.quantity = quantity;
        }
    }
}
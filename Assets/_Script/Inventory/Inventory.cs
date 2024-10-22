using System.Collections.Generic;
using UnityEngine;

namespace _Script.Items
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private int capacity = 20;
        [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();

        public bool AddItem(ItemData itemData, int quantity)
        {
            if (itemData.maxStackSize > 1)
            {
                // Try to find an existing stack
                InventoryItem existingItem = items.Find(item => item.itemData == itemData);
                if (existingItem != null)
                {
                    existingItem.quantity += quantity;
                    if (existingItem.quantity > itemData.maxStackSize)
                    {
                        int excess = existingItem.quantity - itemData.maxStackSize;
                        existingItem.quantity = itemData.maxStackSize;
                        return AddItem(itemData, excess);
                    }
                    return true;
                }
            }

            if (items.Count >= capacity)
            {
                Debug.Log("Inventory is full!");
                return false;
            }

            items.Add(new InventoryItem(itemData, quantity));
            return true;
        }

        public bool RemoveItem(ItemData itemData, int quantity)
        {
            InventoryItem existingItem = items.Find(item => item.itemData == itemData);
            if (existingItem != null)
            {
                existingItem.quantity -= quantity;
                if (existingItem.quantity <= 0)
                {
                    items.Remove(existingItem);
                }
                return true;
            }
            return false;
        }
        
        public void UseItem(ItemData itemData)
        {
            // Implement item usage logic
        }
    }

}
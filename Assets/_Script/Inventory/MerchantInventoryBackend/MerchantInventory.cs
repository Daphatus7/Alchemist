// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 54

using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.MerchantInventoryBackend
{
    public class MerchantInventory : InventoryBackend.Inventory
    {
        [Header("Merchant Settings")]
        [SerializeField] private string merchantName;
        [SerializeField] private List<InventoryItem> itemsForSale;

        public List<ItemData> debugItems;
        
        public string MerchantName => merchantName;
        
        private void Start()
        {
            foreach (var item in debugItems)
            {
                AddItem(new InventoryItem(item, item.MaxStackSize));
            }
            
            InitializeMerchantInventory();
        }

        /// <summary>
        /// Initialize the merchant's inventory with predefined items for sale.
        /// </summary>
        private void InitializeMerchantInventory()
        {
            // Clear and populate inventory slots with items for sale
            for (int i = 0; i < itemsForSale.Count && i < Capacity; i++)
            {
                AddItemToSlot(itemsForSale[i], i);
            }
        }

        public override SlotType SlotType => SlotType.Merchant;

        public override void LeftClickItem(int slotIndex)
        {
            
        }
    }
    

}

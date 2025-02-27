// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 54

using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.ItemInstance;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.MerchantInventoryBackend
{
    public class MerchantInventory : InventoryBackend.Inventory
    {
        private readonly List<ItemInstance.ItemInstance> _itemsForSale;
        
        public MerchantInventory(List<ItemInstance.ItemInstance> itemsForSale, int width = 5, int height = 4) : base(width, height)
        {
            _itemsForSale = itemsForSale;
            InitializeMerchantInventory();
        }
        
        public MerchantInventory(ItemSave [] itemSaves, int width = 5, int height = 4) : base(width, height)
        {
            _itemsForSale = new List<ItemInstance.ItemInstance>();
            InitializeMerchantInventory();
        }
        
        public MerchantInventory(InventorySave save) : base(save)
        {
            
        }
        
        /// <summary>
        /// Initialize the merchant's inventory with predefined items for sale.
        /// </summary>
        private void InitializeMerchantInventory()
        {
            // Clear and populate inventory slots with items for sale
            foreach (var t in _itemsForSale)
            {
                AddItem(t);
            }
        }

        public override SlotType SlotType => SlotType.Merchant;

        public override void LeftClickItem(int slotIndex)
        {
            
        }
    }
    

}

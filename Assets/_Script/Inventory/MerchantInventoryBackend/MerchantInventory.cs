// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 54

using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.MerchantInventoryBackend
{
    public class MerchantInventory : InventoryBackend.Inventory
    {
        private List<InventoryItem> _itemsForSale;
        
        public MerchantInventory(int capacity, List<InventoryItem> itemsForSale) : base(capacity)
        {
            this._itemsForSale = itemsForSale;
            InitializeMerchantInventory();
        }

        public MerchantInventory(int capacity, InventoryItem[] items, List<InventoryItem> itemsForSale) : base(capacity, items)
        {
            this._itemsForSale = itemsForSale;
            InitializeMerchantInventory();

        }
        
        /// <summary>
        /// Initialize the merchant's inventory with predefined items for sale.
        /// </summary>
        private void InitializeMerchantInventory()
        {
            // Clear and populate inventory slots with items for sale
            for (int i = 0; i < _itemsForSale.Count && i < Capacity; i++)
            {
                AddItemToSlot(_itemsForSale[i], i);
            }
        }

        public override SlotType SlotType => SlotType.Merchant;

        public override void LeftClickItem(int slotIndex)
        {
            
        }
    }
    

}

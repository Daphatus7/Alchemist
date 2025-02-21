// Author : Peiyu Wang @ Daphatus
// 30 01 2025 01 44

using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Inventory.InventoryBackend
{
    public class InventorySlot
    {
        private ItemInstance.ItemInstance _itemInstance = null;

        public bool IsEmpty => _itemInstance == null;

        /// <summary>
        /// Get or set the entire ItemStack in this slot.
        /// Setting to null or an empty stack means the slot is empty.
        /// </summary>
        public ItemInstance.ItemInstance ItemInstance
        {
            get => _itemInstance;
            set => _itemInstance = value;
        }
        
        /// <summary>
        /// Clears out the slot, making it empty.
        /// </summary>
        public void Clear()
        {
            _itemInstance = null;
        }
    }
}
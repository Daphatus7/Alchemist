// Author : Peiyu Wang @ Daphatus
// 30 01 2025 01 44

using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Inventory.InventoryBackend
{
    public class InventorySlot
    {
        private ItemStack _itemStack = null;

        public bool IsEmpty => _itemStack == null || _itemStack.IsEmpty;

        /// <summary>
        /// Get or set the entire ItemStack in this slot.
        /// Setting to null or an empty stack means the slot is empty.
        /// </summary>
        public ItemStack ItemStack
        {
            get => _itemStack;
            set => _itemStack = value;
        }

        /// <summary>
        /// Convenience property for referencing the slot's item data directly.
        /// </summary>
        public ItemData ItemData => _itemStack?.ItemData;
        

        /// <summary>
        /// Clears out the slot, making it empty.
        /// </summary>
        public void Clear()
        {
            _itemStack = null;
        }
    }
}
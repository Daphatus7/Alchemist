using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    [System.Serializable]
    public class ItemStack
    {
        public ItemData ItemData { get; private set; }
        public int Quantity { get; set; }

        public bool IsEmpty => ItemData == null || Quantity <= 0;

        public ItemStack(ItemStack stack)
        {
            if (stack == null || stack.ItemData == null || stack.Quantity <= 0)
            {
                Clear();
            }
            else
            {
                ItemData = stack.ItemData;
                Quantity = Mathf.Clamp(stack.Quantity, 0, stack.ItemData.MaxStackSize);
            }
        }

        public ItemStack()
        {
            Clear();
        }

        public ItemStack(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Clear();
                return;
            }

            ItemData = itemData;
            Quantity = Mathf.Clamp(quantity, 0, itemData.MaxStackSize);
        }

        public void Clear()
        {
            ItemData = null;
            Quantity = 0;
        }

        /// <summary>
        /// 尝试向该堆叠中添加另一个相同类型的堆叠物品。
        /// 返回添加后另一堆剩余的数量，如果能全加完则返回0。
        /// </summary>
        public int TryAdd(ItemStack other)
        {
            if (other == null || other.IsEmpty || other.ItemData != this.ItemData)
                return other?.Quantity ?? 0;

            int space = ItemData.MaxStackSize - Quantity;
            int toAdd = Mathf.Min(space, other.Quantity);
            Quantity += toAdd;
            return other.Quantity - toAdd;
        }

        /// <summary>
        /// 从该堆中拆出指定数量的物品，返回新堆叠。
        /// 如果数量不足，则返回尽可能多的可用物品堆。
        /// 拆出后本堆叠数量减少。
        /// </summary>
        public ItemStack Split(int count)
        {
            if (IsEmpty || count <= 0) 
                return new ItemStack(); //返回空堆叠

            int actualCount = Mathf.Min(count, Quantity);
            Quantity -= actualCount;
            return new ItemStack(ItemData, actualCount);
        }
    }
}
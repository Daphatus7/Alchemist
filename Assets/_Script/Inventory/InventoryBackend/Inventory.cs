using System;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public abstract class Inventory
    {
        private readonly int _capacity;
        public int Capacity => _capacity;
        
        protected ItemStack[] slots;
        public ItemStack[] Slots => slots;
        
        //Load an Empty Inventory
        public Inventory(int capacity)
        {
            _capacity = capacity;
            
            slots = new ItemStack[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                slots[i] = new ItemStack();
            }
        }
        
        //Load an Inventory with Items
        public Inventory(int capacity, ItemStack[] items)
        {
            _capacity = capacity;
            slots = new ItemStack[_capacity];

            // 将传入的 items 复制到 slots 中，如果数量不够则剩余为空
            for (int i = 0; i < _capacity; i++)
            {
                if (items != null && i < items.Length && items[i] != null && !items[i].IsEmpty)
                {
                    // 创建新堆叠，避免对同一堆叠引用的副作用
                    slots[i] = new ItemStack(items[i].ItemData, items[i].Quantity);
                }
                else
                {
                    slots[i] = new ItemStack();
                }
            }
        }
        

        public abstract SlotType SlotType { get; }

        // Event to notify when the inventory has changed
        public event Action<int> OnInventorySlotChanged;
        
        /// <summary>
        /// 尝试向背包中添加物品堆叠的逻辑：
        /// 1. 先尝试向已有相同物品类型的堆叠中合并
        /// 2. 若无法完全合并，则尝试放入空槽
        /// 返回值为未能放入的剩余物品堆叠，如果全部放入则返回null
        /// </summary>
        public ItemStack AddItem(ItemStack itemStackToAdd)
        {
            if (itemStackToAdd == null || itemStackToAdd.IsEmpty)
            {
                return null;
            }

            if (itemStackToAdd.ItemData is ContainerItem)
            {
                //此处根据您的判断逻辑，如果当前Inventory本身是Container的内容（例如判断SlotType是否是Bag，或者您的Container类型判断）
                //假设SlotType为Bag表示是个容器背包，那么禁止将ContainerItem放入其中
                if (this.SlotType == SlotType.Bag)
                {
                    Debug.LogWarning("Cannot place a ContainerItem inside another Container.");
                    return itemStackToAdd; //直接返回，不添加该物品
                }
            }
            

            // 先合并到已有的相同类型堆叠
            for (int i = 0; i < _capacity; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.ItemData == itemStackToAdd.ItemData && slot.Quantity < slot.ItemData.MaxStackSize)
                {
                    int oldQuantity = itemStackToAdd.Quantity;
                    int remaining = slot.TryAdd(itemStackToAdd);
                    if (remaining < oldQuantity)
                        OnInventorySlotChanged?.Invoke(i);

                    if (remaining == 0)
                    {
                        // 全部合并成功
                        return null;
                    }

                    // 仍有剩余未放入的物品
                    itemStackToAdd = new ItemStack(itemStackToAdd.ItemData, remaining);
                }
            }

            // 再尝试放入空槽
            for (int i = 0; i < _capacity; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);
                    slots[i] = new ItemStack(itemStackToAdd.ItemData, toAdd);
                    OnInventorySlotChanged?.Invoke(i);

                    int remaining = itemStackToAdd.Quantity - toAdd;
                    if (remaining <= 0)
                    {
                        return null; // 全部放入成功
                    }
                    itemStackToAdd = new ItemStack(itemStackToAdd.ItemData, remaining);
                }
            }

            // 若到这里仍有剩余则表示背包已满
            return itemStackToAdd;
        }
        
        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }

            if (slots[slotIndex].IsEmpty)
            {
                slots[slotIndex] = itemStack;
                OnInventorySlotChanged?.Invoke(slotIndex);
            }
            else
            {
                Debug.LogWarning("Target slot is not empty, cannot add item directly.");
            }
        }

        protected bool AddItemToSlot(ItemStack itemStackToAdd, int slotIndex)
        {
            if (itemStackToAdd == null || itemStackToAdd.IsEmpty)
            {
                Debug.LogWarning("ItemData is null or stack is empty.");
                return false;
            }

            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            var slot = slots[slotIndex];
            if (!slot.IsEmpty && slot.ItemData == itemStackToAdd.ItemData && slot.Quantity < itemStackToAdd.ItemData.MaxStackSize)
            {
                int remaining = slot.TryAdd(itemStackToAdd);
                OnInventorySlotChanged?.Invoke(slotIndex);
                return remaining < itemStackToAdd.Quantity; // 如果有添加成功则返回true
            }

            if (slot.IsEmpty)
            {
                int toAdd = Mathf.Min(itemStackToAdd.Quantity, itemStackToAdd.ItemData.MaxStackSize);
                slots[slotIndex] = new ItemStack(itemStackToAdd.ItemData, toAdd);
                OnInventorySlotChanged?.Invoke(slotIndex);

                int left = itemStackToAdd.Quantity - toAdd;
                return left < itemStackToAdd.Quantity;
            }

            Debug.Log("Slot is full or not compatible with the item type.");
            return false;
        }

        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }

            if (slots[slotIndex].IsEmpty)
            {
                return null;
            }

            ItemStack removed = new ItemStack(slots[slotIndex].ItemData, slots[slotIndex].Quantity);
            slots[slotIndex].Clear();
            OnInventorySlotChanged?.Invoke(slotIndex);
            return removed;
        }
        
        protected virtual bool RemoveItem(ItemStack itemStack, int quantity = 1)
        {
            if (itemStack == null || itemStack.IsEmpty || quantity <= 0)
            {
                Debug.LogWarning("Cannot remove null or empty stack, or invalid quantity.");
                return false;
            }

            int quantityToRemove = quantity;
            for (int i = 0; i < _capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemData == itemStack.ItemData)
                {
                    int amountToRemove = Math.Min(quantityToRemove, slots[i].Quantity);
                    slots[i].Quantity -= amountToRemove;
                    quantityToRemove -= amountToRemove;

                    if (slots[i].Quantity <= 0)
                    {
                        OnItemUsedUp(i);
                        slots[i].Clear();
                    }
                    OnInventorySlotChanged?.Invoke(i);

                    if (quantityToRemove <= 0)
                    {
                        return true;
                    }
                }
            }

            if (quantityToRemove > 0)
            {
                Debug.Log("Not enough items to remove.");
                return false;
            }

            return true;
        }
        
        protected bool RemoveItemFromSlot(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= _capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            if (slots[slotIndex].IsEmpty)
            {
                Debug.Log("Slot is empty.");
                return false;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove.");
                return false;
            }

            if (slots[slotIndex].Quantity >= quantity)
            {
                slots[slotIndex].Quantity -= quantity;

                if (slots[slotIndex].Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    slots[slotIndex].Clear();
                }

                OnInventorySlotChanged?.Invoke(slotIndex);
                return true;
            }
            else
            {
                Debug.Log("Not enough items in slot to remove.");
                return false;
            }
        }
        
        protected virtual void OnItemUsedUp(int slotIndex)
        {
            // 子类可重写此方法，以实现当某格物品用尽时的特殊逻辑
        }

        /// <summary>
        /// 点击物品槽位(例如左键点击)的逻辑在子类中实现
        /// </summary>
        public abstract void LeftClickItem(int slotIndex);
    }
}
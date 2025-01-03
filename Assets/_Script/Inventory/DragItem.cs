using System;
using _Script.Inventory.InventoryBackend;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Inventory
{
    public class DragItem : Singleton<DragItem>
    {
        private Image _image;
        private ItemStack _itemStack;
        
        protected override void Awake()
        {
            base.Awake();
            _image = GetComponent<Image>();
        }
        
        
        
        public void AddItemToDrag(ItemStack itemStack)
        {
            Debug.Log("Adding item to drag" + itemStack.ItemData.itemName);
            _itemStack = itemStack;
            _image.sprite = itemStack.ItemData.itemIcon;
        }
        
        public ItemStack PeakItemStack()
        {
            return _itemStack;
        }
        
        /**
         * Item is removed from the stack
         */
        public ItemStack RemoveItemStack()
        {
            var result = _itemStack;
            _image.sprite = null;
            _itemStack = null;
            return result;
        }
    }
}
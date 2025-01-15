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
        private bool _isDragItemRotated;
        private RectTransform _rectTransform;
        
        protected override void Awake()
        {
            base.Awake();
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            //R to rotate
            if (_itemStack != null && Input.GetKeyDown(KeyCode.R))
            {
                RotateDragItem();
            }
        }
        
        private void RotateDragItem()
        {
            _isDragItemRotated = !_isDragItemRotated;
            var isRotated = _itemStack.ItemData.ItemShape.ToggleRotate();
            _rectTransform.localRotation = Quaternion.Euler(0, 0, isRotated ? -90 : 0);
            //Update the sprite
        }
        
        public void AddItemToDrag(ItemStack itemStack)
        {
            Debug.Log("Adding item to drag" + itemStack.ItemData.itemName);
            _itemStack = itemStack;
            _image.sprite = itemStack.ItemData.itemIcon;
            //reset rotation
            _rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
            _isDragItemRotated = false;
            if(itemStack.ItemData.ItemShape.IsRotated)
            {
                RotateDragItem();
            }
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

        public ItemStack RemoveItemStackOnFail()
        {
            if(_isDragItemRotated)
            {
                _itemStack.ItemData.ItemShape.ToggleRotate();
            }
            var result = _itemStack;
            _image.sprite = null;
            _itemStack = null;
            return result;
        }
    }
}
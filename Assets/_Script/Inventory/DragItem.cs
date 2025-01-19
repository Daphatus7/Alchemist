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
        
        //the position where the player initiate the drag action
        //Updated when the player start dragging
        private Vector2Int _dragStartPosition;
        //slot position 的index
        private int _dragOnItemSlotIndex; public int DragOnItemSlotIndex => _dragOnItemSlotIndex;
        
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
            var isRotated = _itemStack.ToggleRotate(_dragStartPosition, _isDragItemRotated);
            _isDragItemRotated = !_isDragItemRotated;
            _rectTransform.localRotation = Quaternion.Euler(0, 0, isRotated ? - 90 : 0);
            //Update the sprite
        }
        
        public void AddItemToDrag(ItemStack itemStack, Vector2Int dragStartPosition)
        {
            Debug.Log("Adding item to drag" + itemStack.ItemData.itemName);
            _dragStartPosition = dragStartPosition;
            var relativePosition = dragStartPosition - itemStack.PivotPosition;
            _dragOnItemSlotIndex = itemStack.ItemData.ItemShape.GetSelectedSlotIndex(relativePosition);
            Debug.Log("Pivot position: " + _dragStartPosition);
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
                _itemStack.ToggleRotate(_dragStartPosition,_isDragItemRotated);
            }
            var result = _itemStack;
            _image.sprite = null;
            _itemStack = null;
            return result;
        }
    }
}
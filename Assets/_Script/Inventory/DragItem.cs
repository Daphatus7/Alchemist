using System;
using System.Collections.Generic;
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
        
        //鼠标选中的位置
        private Vector2Int _dragStartPosition;
        //slot position 的index
        private Vector2Int _dragRelativePosition; public Vector2Int DragRelativePosition => _dragRelativePosition;
        
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
            _itemStack = itemStack;
            _image.sprite = itemStack.ItemData.itemIcon;
            _dragStartPosition = dragStartPosition; //
            _dragRelativePosition = dragStartPosition - itemStack.ItemData.ItemShape.Positions[0];
                
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

        public List<Vector2Int> ProjectedPositions(Vector2Int targetSlotPosition //放置的位置
        )
        {
            var projectedPositions = new List<Vector2Int>();
            var inventoryOffset = _itemStack.ItemPositions;
            var shiftVector = targetSlotPosition - _dragStartPosition;
            
            //position after shift
            for(int i = 0; i < inventoryOffset.Count; i++)
            {
                var projectedPosition = inventoryOffset[i] + shiftVector;
                projectedPositions.Add(projectedPosition);
            }
            return projectedPositions;
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
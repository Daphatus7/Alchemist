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
        private bool _isDragItemRotated = false;
        private RectTransform _rectTransform;
        
        //the position where the player initiate the drag action
        //Updated when the player start dragging
        
        //鼠标选中的位置
        private Vector2Int _dragStartPosition;
        //slot position 的index
        
        /// <summary>
        /// 物品的原始位置，在拖拽起始时记录
        /// </summary>
        
        /// <summary>
        /// 选中的位置相对于item的位置
        /// </summary>
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
            //Rotate the data
            var isRotated = _itemStack.ToggleRotate(_dragStartPosition);
            _isDragItemRotated = !_isDragItemRotated;
            
            //Rotate the drag display
            _rectTransform.localRotation = Quaternion.Euler(0, 0, isRotated ? - 90 : 0);
            //Update the sprite
        }
        
        public void AddItemToDrag(ItemStack itemStack, Vector2Int dragStartPosition)
        {
            _itemStack = itemStack;
            _image.sprite = itemStack.ItemData.itemIcon;
            _dragStartPosition = dragStartPosition; //
            _isDragItemRotated = false;
            _dragRelativePosition = dragStartPosition - itemStack.ItemData.ItemShape.Positions[0];
            //reset rotation
            _rectTransform.localRotation = Quaternion.Euler(0, 0, itemStack.IsRotated ? -90 : 0);
        }
        
        public ItemStack PeakItemStack()
        {
            return _itemStack;
        }

        /// <summary>
        /// 获取想放置的位置
        /// </summary>
        /// <param name="targetSlotPosition"></param>
        /// <returns></returns>
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
                Debug.Log("projectedPosition: " + projectedPosition);
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
            if (_isDragItemRotated)
            {
                _itemStack.ToggleRotate(_dragStartPosition);
                
                foreach(var position in _itemStack.ItemPositions)
                {
                    Debug.Log("position: " + position);
                }
            }
            var result = _itemStack;
            
            _image.sprite = null;
            _itemStack = null;
            return result;
        }
    }
}
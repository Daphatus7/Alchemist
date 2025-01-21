// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 31

using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using _Script.UserInterface;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontendBase
{
    public abstract class InventoryUIBase<TInventory> : 
        MonoBehaviour, IContainerUIHandle, IUIHandler
        where TInventory : InventoryBackend.Inventory
    {
        [SerializeField] protected TInventory inventory; public TInventory Inventory => inventory;
        [SerializeField] protected GameObject inventoryPanel;
        [SerializeField] protected GameObject slotPrefab;
        [SerializeField] private GameObject slotVisualPrefab;
        [SerializeField] private GameObject slotVisualParent;
        
        private readonly List<InventorySlotDisplay> _slotUIs = new List<InventorySlotDisplay>();

        public const int CellSize = 50;
        
        protected InventorySlotInteraction[] _slotInteractions;

        protected virtual void Awake()
        {
            //cause problems with loading sequence
            gameObject.SetActive(false);
        }
        
        protected virtual void Start()
        {
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
            inventory.OnItemStackChanged -= RenderSlotIcons;
            inventory.OnItemStackChanged += RenderSlotIcons;
            
            inventory.OnInventorySlotChanged -= UpdateSlotUI;
            inventory.OnInventorySlotChanged += UpdateSlotUI;
        }
        
        public void HideUI()
        {
            inventory.OnItemStackChanged -= RenderSlotIcons;
            inventory.OnInventorySlotChanged -= UpdateSlotUI;
            
            gameObject.SetActive(false);
        }
        
        protected virtual void AssignInventory(TInventory inventory)
        {
            this.inventory = inventory;
        }

        protected virtual void ClearInventory()
        {
            //remove inventory slots
            inventory = null;
            ClearAllSlotsUI();
        }
        
        private void OnDestroy()
        {
            inventory.OnInventorySlotChanged -= UpdateSlotUI;
            inventory.OnItemStackChanged -= RenderSlotIcons;
        }
        
        
        protected virtual void InitializeInventoryUI()
        {
            // Clear the inventory panel
            foreach (Transform child in inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            _slotInteractions = new InventorySlotInteraction[inventory.Capacity];
            
            
            for (int i = 0; i < inventory.Capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotInteraction inventorySlotInteraction = slot.GetComponent<InventorySlotInteraction>();
                // Initialize the slot
                inventorySlotInteraction.InitializeInventorySlot(this, i, inventory.SlotType);
                _slotInteractions[i] = inventorySlotInteraction;
                // Set the slot's initial item
                inventorySlotInteraction.SetSlot(inventory.GetItemStackAt(i));
            }
            
            for (int i = 0; i < inventory.ItemStacks.Count ; i++)
            {
                _slotInteractions[i].SetSlot(inventory.GetItemStackAt(i));
            }
            
            RenderSlotIcons();
        }
        
        // Update all slots in the UI
        protected void UpdateAllSlotsUI()
        {
            for (int i = 0; i < _slotInteractions?.Length; i++)
            {
                _slotInteractions[i].SetSlot(inventory.GetItemStackAt(i));
            }
        }

        private void ClearAllSlotsUI()
        {
            for (int i = 0; i < _slotInteractions?.Length; i++)
            {
                _slotInteractions[i].ClearSlot();
            }
        }
        
        // Update a single slot in the UI
        protected virtual void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotInteractions.Length)
                return;

            InventorySlotInteraction slotInteraction = _slotInteractions[slotIndex];
            slotInteraction.SetSlot(inventory.GetItemStackAt(slotIndex));
        }
        
        public void OnSlotClicked(InventorySlotInteraction slotInteraction)
        {
            inventory.LeftClickItem(slotInteraction.SlotIndex);
        }
        
        public virtual ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            return inventory.RemoveAllItemsFromSlot(slotIndex);
        }

        public virtual void AddItemToEmptySlot(ItemStack itemStack, List<Vector2Int> peakItemStack)
        {
            inventory.AddItemToEmptySlot(itemStack, peakItemStack);
        }

        public ItemStack AddItem(ItemStack itemStack)
        {
            return inventory.AddItem(itemStack);
        }

        public virtual bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }

        public bool CanFitItem(List<Vector2Int> projectedPositions)
        {
            return inventory.CanFitItem(projectedPositions);
        }
        
        public Vector2Int GetSlotPosition(int slotIndex)
        {
            return inventory.SlotIndexToGrid(slotIndex);
        }
        
        public int GetSlotIndex(Vector2Int position)
        {
            return inventory.GridToSlotIndex(position.x, position.y);
        }

        public int GetItemsCount(int shiftedPivotIndex, List<Vector2Int> peakItemStack, out int onlyItemIndex)
        {
            return inventory.GetItemsCountAtPositions(shiftedPivotIndex, peakItemStack, out onlyItemIndex);
        }
        
        /// <summary>
        /// Checks if two ItemStacks represent the same item type.
        /// Returns true if both are non-empty and share the same ItemData, otherwise false.
        /// </summary>
        public bool CompareItems(ItemStack item1, ItemStack item2)
        {
            if (item1 == null || item1.IsEmpty || item2 == null || item2.IsEmpty) return false;
            return item1.ItemData == item2.ItemData;
        }


        #region Inventory Renderer

        // A local cache: slotIndex -> the UI GameObject we created


        protected void RenderSlotIcons()
        {
            // Clean up old if any
            foreach (var slotUI in _slotUIs)
            {
                Destroy(slotUI.gameObject);
            }
            
            _slotUIs.Clear();
            
            
            foreach (var item in inventory.ItemStacks)
            {
                if (item == null || item.IsEmpty) continue;

                var newItemDisplay = Instantiate(slotVisualPrefab, slotVisualParent.transform);
                var rect = newItemDisplay.GetComponent<RectTransform>();
                
                // Use anchoredPosition on the newItem's RectTransform
                var itemSize =item.ItemData.ItemShape.IconScale;
                if(item.IsRotated)
                {
                    rect.Rotate(0, 0, -90);
                }
                
                rect.anchoredPosition = GetSlotVisualPosition(item.RenderingPivot // pivot的位置可能需要手动设置
                    , itemSize);
                //modify the width and height of the slot
                
                //debug item size
                rect.sizeDelta = new Vector2(CellSize * itemSize.x, CellSize * itemSize.y);
                var renderingOffset = item.RenderingOffset;
                rect.localPosition = new Vector3(rect.localPosition.x + renderingOffset.x, rect.localPosition.y + renderingOffset.y, 0);
                var slotUI = newItemDisplay.GetComponent<InventorySlotDisplay>();
                slotUI.SetDisplay(item.ItemData, item.Quantity);
                _slotUIs.Add(slotUI);
            }
        }

        private Vector2 GetSlotVisualPosition(Vector2Int pivotPosition, Vector2 itemSize)
        {
            // If row 0 is top, invert y
            var cellSizeX = CellSize * itemSize.x;
            var cellSizeY = CellSize * itemSize.y;
            var posX = pivotPosition.y * CellSize + cellSizeX/2;
            var posY = - cellSizeY/2 - pivotPosition.x * CellSize;

            return new Vector2(posX, posY);
        }
        #endregion
    }
}

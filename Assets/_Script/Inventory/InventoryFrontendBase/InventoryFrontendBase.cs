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

        
        protected InventorySlotInteraction[] _slotDisplays;

        protected void Awake()
        {
            gameObject.SetActive(false);
        }
        
        protected virtual void Start()
        {
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
            inventory.OnItemStackChanged += UpdateItemStacks;
        }
        
        public void HideUI()
        {
            inventory.OnItemStackChanged -= UpdateItemStacks;
            gameObject.SetActive(false);
        }
        
        protected virtual void AssignInventory(TInventory inventory)
        {
            this.inventory = inventory;
            inventory.OnInventorySlotChanged += UpdateSlotUI;
        }

        protected virtual void ClearInventory()
        {
            //remove inventory slots
            inventory.OnInventorySlotChanged -= UpdateSlotUI;
            inventory = null;
            ClearAllSlotsUI();
        }
        
        private void OnDestroy()
        {
            inventory.OnInventorySlotChanged -= UpdateSlotUI;
        }
        
        
        protected virtual void InitializeInventoryUI()
        {
            // Clear the inventory panel
            foreach (Transform child in inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            _slotDisplays = new InventorySlotInteraction[inventory.Capacity];
            
            
            for (int i = 0; i < inventory.Capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotInteraction inventorySlotInteraction = slot.GetComponent<InventorySlotInteraction>();
                // Initialize the slot
                inventorySlotInteraction.InitializeInventorySlot(this, i, inventory.SlotType);
                _slotDisplays[i] = inventorySlotInteraction;
                // Set the slot's initial item
                inventorySlotInteraction.SetSlot(inventory.GetItemStackAt(i));
            }
            
            for (int i = 0; i < inventory.ItemStacks.Count ; i++)
            {
                _slotDisplays[i].SetSlot(inventory.GetItemStackAt(i));
            }
        }
        
        // Update all slots in the UI
        protected void UpdateAllSlotsUI()
        {
            for (int i = 0; i < _slotDisplays?.Length; i++)
            {
                _slotDisplays[i].SetSlot(inventory.GetItemStackAt(i));
            }
        }

        private void ClearAllSlotsUI()
        {
            for (int i = 0; i < _slotDisplays?.Length; i++)
            {
                _slotDisplays[i].ClearSlot();
            }
        }
        
        // Update a single slot in the UI
        protected virtual void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotDisplays.Length)
                return;

            InventorySlotInteraction slotInteraction = _slotDisplays[slotIndex];
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

        public virtual void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            inventory.AddItemToEmptySlot(itemStack, slotIndex);
        }

        public ItemStack AddItem(ItemStack itemStack)
        {
            return inventory.AddItem(itemStack);
        }

        public virtual bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }

        public bool CanFitItem(int targetSlotIndex, ItemStack comparingItemStack)
        {
            return inventory.CanFitItem(targetSlotIndex, comparingItemStack);
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
        

        /// <summary>
        /// Creates UI objects for all slots in the inventory.
        /// Typically called once at start or inventory re-init.
        /// </summary>
        protected void CreateVisualSlots()
        {
            // Clean up old if any
            foreach (var slotUI in _slotUIs)
            {
                Destroy(slotUI);
            }
            _slotUIs.Clear();

            UpdateItemStacks();
        }

        private void UpdateItemStacks()
        {
            Debug.Log("Updating item stacks");
            foreach (var item in inventory.ItemStacks)
            {
                var newItemDisplay = Instantiate(slotVisualPrefab, slotVisualParent.transform);
                newItemDisplay.transform.localPosition = Vector3.zero;
                //May need to update location later
                var slotUI = newItemDisplay.GetComponent<InventorySlotDisplay>();
                slotUI.SetSlotImage(item.ItemData.ItemSprite);
                _slotUIs.Add(slotUI);
            }
        }
        #endregion
        
    }
}

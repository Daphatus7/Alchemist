using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontend
{
    public class InventoryUI : MonoBehaviour, IPlayerInventoryHandler
    {
        private PlayerContainer _playerContainer;
        public PlayerContainer CurrentContainer => _playerContainer;
        
        private IInventoryManagerHandler _inventoryManager;
        
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;

        private InventorySlotInteraction[] _slotDisplays;
        private bool _initialized = false;

        /// <summary>
        /// Checks if two ItemStacks represent the same item type.
        /// Returns true if both are non-empty and share the same ItemData, otherwise false.
        /// </summary>
        public bool CompareItems(ItemStack item1, ItemStack item2)
        {
            if (item1 == null || item1.IsEmpty || item2 == null || item2.IsEmpty) return false;
            return item1.ItemData == item2.ItemData;
        }
        
        /// <summary>
        /// Toggles the visibility of the inventory UI.
        /// </summary>
        public void ToggleInventoryUI()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void OnDestroy()
        {
            if (_playerContainer != null)
                _playerContainer.OnInventorySlotChanged -= UpdateSlotUI;
        }

        /// <summary>
        /// Initializes the inventory UI with the given inventory manager and player container,
        /// then creates slot displays for each slot.
        /// </summary>
        public void InitializeInventoryUI(IInventoryManagerHandler inventoryManager, PlayerContainer playerContainer)
        {
            Debug.Log("Initializing inventory UI for " + playerContainer.UniqueID);
            _playerContainer = playerContainer;
            _inventoryManager = inventoryManager;
            _playerContainer.OnInventorySlotChanged += UpdateSlotUI;

            int capacity = _playerContainer.Capacity;
            _slotDisplays = new InventorySlotInteraction[capacity];

            // Initialize the slot pool if not yet done (implementation not shown).
            if (!_initialized)
            {
                // Example:
                // SlotPool.Initialize(slotPrefab, inventoryPanel.transform, 100);
                _initialized = true;
            }

            for (int i = 0; i < capacity; i++)
            {
                InventorySlotInteraction inventorySlotInteraction = SlotPool.GetSlot();
                inventorySlotInteraction.transform.SetParent(inventoryPanel.transform, false);
                inventorySlotInteraction.transform.localScale = Vector3.one;
                inventorySlotInteraction.InitializeInventorySlot(this, i, _playerContainer.SlotType);
                inventorySlotInteraction.gameObject.SetActive(true);
                _slotDisplays[i] = inventorySlotInteraction;

                // Set the initial item stack for this slot
                inventorySlotInteraction.SetSlot(_playerContainer.GetItemStackAt(i));
            }

            // Optionally update all slots here to ensure full sync
            UpdateAllSlotsUI();
        }
        
        /// <summary>
        /// Closes the inventory UI, returns its slots to the pool, unsubscribes events, 
        /// and destroys the UI game object.
        /// </summary>
        public void CloseInventoryUI()
        {
            for (int i = 0; i < _slotDisplays.Length; i++)
            {
                _slotDisplays[i].ClearSlot();
                SlotPool.ReturnSlotToPool(_slotDisplays[i]);
                _slotDisplays[i] = null;
            }

            _playerContainer.OnInventorySlotChanged -= UpdateSlotUI;
            _inventoryManager.OnCloseInventoryUI(this);

            _playerContainer = null;
            _inventoryManager = null;

            Destroy(gameObject);
        }

        /// <summary>
        /// Updates the UI for all slots. Calls SetSlot for each slot display 
        /// to ensure it reflects the current state of the container.
        /// </summary>
        private void UpdateAllSlotsUI()
        {
            if (_slotDisplays == null) return;
            for (int i = 0; i < _slotDisplays.Length; i++)
            {
                _slotDisplays[i].SetSlot(_playerContainer.GetItemStackAt(i));
            }
        }

        /// <summary>
        /// Updates the UI for a specific slot when the container reports a change.
        /// </summary>
        private void UpdateSlotUI(int slotIndex)
        {
            if (_slotDisplays == null || slotIndex < 0 || slotIndex >= _slotDisplays.Length)
                return;

            InventorySlotInteraction slotInteraction = _slotDisplays[slotIndex];
            slotInteraction.SetSlot(_playerContainer.GetItemStackAt(slotIndex));
        }
        
        /// <summary>
        /// Called when a slot is clicked. Forwards the event to the player container 
        /// to handle item usage or selection logic.
        /// </summary>
        public void OnSlotClicked(InventorySlotInteraction slotInteraction)
        {
            _playerContainer.LeftClickItem(slotInteraction.SlotIndex);
        }

        /// <summary>
        /// Removes all items from the specified slot and returns the item stack that was removed.
        /// </summary>
        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            return _playerContainer.RemoveAllItemsFromSlot(slotIndex);
        }

        /// <summary>
        /// Adds the specified ItemStack into an empty slot at the given index.
        /// </summary>
        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            _playerContainer.AddItemToEmptySlot(itemStack, slotIndex);
        }

        /// <summary>
        /// Attempts to add an ItemStack to the inventory. 
        /// Returns null if fully inserted, or the leftover if not enough space was available.
        /// </summary>
        public ItemStack AddItem(ItemStack itemStack)
        {
            return _playerContainer.AddItem(itemStack);
        }
        
        /// <summary>
        /// Determines if this container accepts the given ItemStack.
        /// Currently always returns true. Override if necessary.
        /// </summary>
        public bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }

        public bool CanFitItem(List<Vector2Int> projectedPositions)
        {
            return _playerContainer.CanFitItem(projectedPositions);
        }

        /// <summary>
        /// Increases the player's gold by the specified amount.
        /// </summary>
        public void AddGold(int amount)
        {
            _playerContainer.InventoryOwner.AddGold(amount);
        }

        /// <summary>
        /// Decreases the player's gold by the specified amount and returns true if successful.
        /// </summary>
        public bool RemoveGold(int amount)
        {
            return _playerContainer.InventoryOwner.RemoveGold(amount);
        }
        
        public Vector2Int GetSlotPosition(int slotIndex)
        {
            return _playerContainer.SlotIndexToGrid(slotIndex);
        }
        
        public int GetSlotIndex(Vector2Int position)
        {
            return _playerContainer.GridToSlotIndex(position.x, position.y);
        }

        public int GetItemsCount(int shiftedPivotIndex, List<Vector2Int> itemPositions, out int onlyItemIndex)
        {
            return _playerContainer.GetItemsCountAtPositions(shiftedPivotIndex, itemPositions, out onlyItemIndex);
        }
    }
}
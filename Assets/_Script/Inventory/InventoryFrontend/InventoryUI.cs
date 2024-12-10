using System;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontend
{
    public class InventoryUI : MonoBehaviour, IPlayerInventoryHandler
    {
        private PlayerContainer _playerContainer; public PlayerContainer CurrentContainer => _playerContainer;
        private IInventoryManagerHandler _inventoryManager;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;
        
        
        
        public bool CompareItems(InventoryItem item1, InventoryItem item2)
        {
            return item1.ItemData == item2.ItemData;
        }
        
        
        private InventorySlotDisplay[] _slotDisplays;

        private bool _initialized = false;

        public void ToggleInventoryUI()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void OnDestroy()
        {
            if (_playerContainer != null) 
                _playerContainer.OnInventorySlotChanged -= UpdateSlotUI;
        }

        public void InitializeInventoryUI(IInventoryManagerHandler inventoryManager, PlayerContainer playerContainer)
        {
            _playerContainer = playerContainer;
            _inventoryManager = inventoryManager;
            _playerContainer.OnInventorySlotChanged += UpdateSlotUI;
            
            int capacity = _playerContainer.Capacity;
            _slotDisplays = new InventorySlotDisplay[capacity];

            // Initialize pool if not done yet
            if (!_initialized)
            {
                // For example, initialize the slot pool with some capacity if not done globally:
                // SlotPool.Initialize(slotPrefab, someParentTransform, initialCount: 100);
                // In a production scenario, you'd ensure SlotPool is initialized elsewhere once.
                _initialized = true;
            }

            for (int i = 0; i < capacity; i++)
            {
                InventorySlotDisplay inventorySlotDisplay = SlotPool.GetSlot();
                inventorySlotDisplay.transform.SetParent(inventoryPanel.transform, false);
                inventorySlotDisplay.transform.localScale = Vector3.one;
                inventorySlotDisplay.InitializeInventorySlot(this, i, _playerContainer.SlotType);
                inventorySlotDisplay.gameObject.SetActive(true);
                _slotDisplays[i] = inventorySlotDisplay;

                // Set the slot's initial item
                inventorySlotDisplay.SetSlot(_playerContainer.Slots[i]);
            }
        }
        
        public void CloseInventoryUI()
        {
            // Instead of destroying slots, return them to the pool
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
            
            Destroy(gameObject); // Destroy only the UI container, slots are reused
        }

        private void UpdateAllSlotsUI()
        {
            for (int i = 0; i < _slotDisplays?.Length; i++)
            {
                _slotDisplays[i].SetSlot(_playerContainer.Slots[i]);
            }
        }

        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotDisplays.Length)
                return;

            InventorySlotDisplay slotDisplay = _slotDisplays[slotIndex];
            slotDisplay.SetSlot(_playerContainer.Slots[slotIndex]);
        }
        
        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            _playerContainer.LeftClickItem(slotDisplay.SlotIndex);
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            return _playerContainer.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            _playerContainer.AddItemToEmptySlot(item, slotIndex);
        }

        public InventoryItem AddItem(InventoryItem item)
        {
            return _playerContainer.AddItem(item);
        }
        
        public bool AcceptsItem(InventoryItem item)
        {
            return true;
        }

        public void AddGold(int amount)
        {
            _playerContainer.InventoryOwner.AddGold(amount);
        }

        public bool RemoveGold(int amount)
        {
            return _playerContainer.InventoryOwner.RemoveGold(amount);
        }
    }
}

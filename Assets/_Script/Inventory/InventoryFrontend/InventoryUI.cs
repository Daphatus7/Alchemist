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
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;

        private InventorySlotDisplay[] _slotDisplays;
        
        private void Start()
        {
            InitializeInventoryUI();
            gameObject.SetActive(false);
        }

        public void ToggleInventoryUI()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
        
        
        private void OnEnable()
        {
            _playerContainer.OnInventorySlotChanged += UpdateSlotUI;
            // Subscribe to full inventory updates if needed
            // playerInventory.OnInventoryChanged += UpdateAllSlotsUI;
            UpdateAllSlotsUI();
        }

        private void OnDisable()
        {
            _playerContainer.OnInventorySlotChanged -= UpdateSlotUI;
            // Unsubscribe from full inventory updates if needed
            // playerInventory.OnInventoryChanged -= UpdateAllSlotsUI;
        }
        
        
        private void InitializeInventoryUI()
        {
            int capacity = _playerContainer.Capacity;
            _slotDisplays = new InventorySlotDisplay[capacity];

            for (int i = 0; i < capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                inventorySlotDisplay.InitializeInventorySlot(this, i, _playerContainer.SlotType);
                _slotDisplays[i] = inventorySlotDisplay;

                // Set the slot's initial item
                inventorySlotDisplay.SetSlot(_playerContainer.Slots[i]);
            }
        }



        // Implement if full inventory updates are necessary
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

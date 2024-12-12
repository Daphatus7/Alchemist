// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 31

using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontendBase
{
    public abstract class InventoryUIBase<TInventory> : 
        MonoBehaviour, IContainerUIHandle
        where TInventory : InventoryBackend.Inventory
    {
        [SerializeField] protected TInventory _inventory;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;

        private InventorySlotDisplay[] _slotDisplays;
        
        protected virtual void Start()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowInventory()
        {
            gameObject.SetActive(true);
        }
        
        public void HideInventory()
        {
            gameObject.SetActive(false);
        }
        
        protected virtual void AssignInventory(TInventory inventory)
        {
            _inventory = inventory;
            _inventory.OnInventorySlotChanged += UpdateSlotUI;
            InitializeInventoryUI();
        }

        protected virtual void ClearInventory()
        {
            //remove inventory slots
            _inventory.OnInventorySlotChanged -= UpdateSlotUI;
            _inventory = null;
            ClearAllSlotsUI();
        }
        
        private void OnDestroy()
        {
            _inventory.OnInventorySlotChanged -= UpdateSlotUI;
        }
        
        
        protected void InitializeInventoryUI()
        {
            // Clear the inventory panel
            foreach (Transform child in inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            int capacity = _inventory.Capacity;
            _slotDisplays = new InventorySlotDisplay[capacity];

            for (int i = 0; i < capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                inventorySlotDisplay.InitializeInventorySlot(GetContainerUIHandler(), i, _inventory.SlotType);
                _slotDisplays[i] = inventorySlotDisplay;
                // Set the slot's initial item
                inventorySlotDisplay.SetSlot(_inventory.Slots[i]);
            }
        }
        
        protected virtual IContainerUIHandle GetContainerUIHandler()
        {
            return this;
        }
        
        
        // Update all slots in the UI
        protected void UpdateAllSlotsUI()
        {
            for (int i = 0; i < _slotDisplays?.Length; i++)
            {
                _slotDisplays[i].SetSlot(_inventory.Slots[i]);
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
        protected void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotDisplays.Length)
                return;

            InventorySlotDisplay slotDisplay = _slotDisplays[slotIndex];
            slotDisplay.SetSlot(_inventory.Slots[slotIndex]);
        }
        
        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            _inventory.LeftClickItem(slotDisplay.SlotIndex);
        }
        
        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            return _inventory.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            _inventory.AddItemToEmptySlot(itemStack, slotIndex);
        }

        public ItemStack AddItem(ItemStack itemStack)
        {
            return _inventory.AddItem(itemStack);
        }

        public virtual bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }
    }
}

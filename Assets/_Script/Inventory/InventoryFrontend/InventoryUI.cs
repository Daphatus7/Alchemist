using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontend
{
    public class InventoryUI : MonoBehaviour, IInventoryUIHandle
    {
        [SerializeField] private InventoryBackend.Inventory playerInventory;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;

        private InventorySlotDisplay[] slotDisplays;

        private void Start()
        {
            InitializeInventoryUI();
        }

        private void OnEnable()
        {
            playerInventory.OnInventorySlotChanged += UpdateSlotUI;
            // Subscribe to full inventory updates if needed
            // playerInventory.OnInventoryChanged += UpdateAllSlotsUI;
        }

        private void OnDisable()
        {
            playerInventory.OnInventorySlotChanged -= UpdateSlotUI;
            // Unsubscribe from full inventory updates if needed
            // playerInventory.OnInventoryChanged -= UpdateAllSlotsUI;
        }

        private void InitializeInventoryUI()
        {
            int capacity = playerInventory.Capacity;
            slotDisplays = new InventorySlotDisplay[capacity];

            for (int i = 0; i < capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                inventorySlotDisplay.InitializeInventorySlot(this, i);
                slotDisplays[i] = inventorySlotDisplay;

                // Set the slot's initial item
                inventorySlotDisplay.SetSlot(playerInventory.Slots[i].Item);
            }
        }

        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotDisplays.Length)
                return;

            InventorySlotDisplay slotDisplay = slotDisplays[slotIndex];
            slotDisplay.SetSlot(playerInventory.Slots[slotIndex].Item);
        }

        // Implement if full inventory updates are necessary
        private void UpdateAllSlotsUI()
        {
            for (int i = 0; i < slotDisplays.Length; i++)
            {
                slotDisplays[i].SetSlot(playerInventory.Slots[i].Item);
            }
        }

        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            playerInventory.LeftClickItem(slotDisplay.SlotIndex);
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            return playerInventory.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            playerInventory.AddItemToEmptySlot(item, slotIndex);
        }
    }
}

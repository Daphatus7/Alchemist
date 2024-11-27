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
        
        void Start()
        {
            UpdateInventoryUI();
        }
        
        private void OnEnable()
        {
            playerInventory.OnInventoryChanged += UpdateInventoryUI;
            UpdateInventoryUI();
        }

        private void OnDisable()
        {
            playerInventory.OnInventoryChanged -= UpdateInventoryUI;
        }
        
        private void UpdateInventoryUI()
        {
            foreach (Transform child in inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < playerInventory.Capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                inventorySlotDisplay.InitializeInventorySlot(this, i);
                if (i < playerInventory.Slots.Length)
                {
                    inventorySlotDisplay.SetSlot(playerInventory.Slots[i].Item);
                }
                else
                {
                    inventorySlotDisplay.ClearSlot();
                }
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
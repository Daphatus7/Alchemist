using _Script.Inventory.ActionBarBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.ActionBarFrontend
{
    public class ActionBarUI : MonoBehaviour, IContainerUIHandle
    {
        [SerializeField] private ActionBar _actionBar;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;

        void Start()
        {
            UpdateInventoryUI();
        }
        
        private void OnEnable()
        {
            _actionBar.OnInventoryChanged += UpdateInventoryUI;
        }

        private void OnDisable()
        {
            _actionBar.OnInventoryChanged -= UpdateInventoryUI;
        }
        
        private void UpdateInventoryUI()
        {
            foreach (Transform child in inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < _actionBar.Capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                inventorySlotDisplay.InitializeInventorySlot(this, i);
                if (i < _actionBar.Slots.Length)
                {
                    inventorySlotDisplay.SetSlot(_actionBar.Slots[i].Item);
                }
                else
                {
                    inventorySlotDisplay.ClearSlot();
                }
            }
        }

        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            _actionBar.LeftClickItem(slotDisplay.SlotIndex);
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            return _actionBar.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            _actionBar.AddItemToEmptySlot(item, slotIndex);
        }
    }
}
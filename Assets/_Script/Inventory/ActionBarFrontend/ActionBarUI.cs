using System.Collections.Generic;
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
        
        private InventorySlotDisplay[] _inventorySlotDisplays;
        
        private InventorySlotDisplay _selectedSlotDisplay;

        private void Start()
        {
            InitializeInventoryUI();
            SelectSlot(0);
        }
        
        private void OnEnable()
        {
            _actionBar.OnInventorySlotChanged += UpdateSlotUI;
            // Subscribe to full inventory updates if needed
            // _actionBar.OnInventoryChanged += UpdateAllSlotsUI;
        }

        private void OnDisable()
        {
            _actionBar.OnInventorySlotChanged -= UpdateSlotUI;
            // Unsubscribe from full inventory updates if needed
            // _actionBar.OnInventoryChanged -= UpdateAllSlotsUI;
        }

        private void InitializeInventoryUI()
        {
            int capacity = _actionBar.Capacity;
            _inventorySlotDisplays = new InventorySlotDisplay[capacity];

            for (int i = 0; i < capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                
                // Initialize the slot
                inventorySlotDisplay.InitializeInventorySlot(this, i);
                inventorySlotDisplay.SetSlot(_actionBar.Slots[i].Item);

                // Store the slot display
                _inventorySlotDisplays[i] = inventorySlotDisplay;
            }
        }

        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventorySlotDisplays.Length)
                return;

            InventorySlotDisplay slotDisplay = _inventorySlotDisplays[slotIndex];
            slotDisplay.SetSlot(_actionBar.Slots[slotIndex].Item);
        }
        
        /// <summary>
        /// Select the slot at the given index.
        /// if the slot is already selected, use the item.
        /// if the previous slot is not empty, deselect it.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void SelectSlot(int slotIndex)
        {
            //Case 1: Invalid slot index
            // do nothing
            if (slotIndex < 0 || slotIndex >= _inventorySlotDisplays.Length)
            {
                Debug.LogWarning("Invalid slot index.");
                return;
            }
            
            //Case 2: Check if has an item
            //if the slot is empty, then try to deselect the previous item
            //and set selected item to null
            if(_actionBar.Slots[slotIndex].IsEmptySlot())
            {
                Debug.LogWarning("No item in slot.");
                //still deselect the previous item
                DeselectPreviousSlot();
                _selectedSlotDisplay = _inventorySlotDisplays[slotIndex];
                _selectedSlotDisplay.HighlightSlot();
                
                _actionBar.OnSelectNone();
                return;
            }
            
            //Case 3: Check if selecting the same slot and is not seed item
            if (_selectedSlotDisplay && _selectedSlotDisplay.SlotIndex == slotIndex && _actionBar.Slots[slotIndex].Item.ItemData.ItemTypeString != "Seed")
            {
                // Use the selected slot
                _actionBar.LeftClickItem(slotIndex);
                return;
            }
            
            /*Deselect previous item*/
            // Unhighlight the previous slot
            DeselectPreviousSlot();
            
            /*Select new item*/
            // Highlight the new slot
            SetSelectedSlot(slotIndex);
            // Update selected slot item
        }
        
        private void SetSelectedSlot(int slotIndex)
        {
            _selectedSlotDisplay = _inventorySlotDisplays[slotIndex];
            _selectedSlotDisplay.HighlightSlot();
            _actionBar.OnSelectItem(slotIndex);
        }

        
        private void DeselectPreviousSlot()
        {
            if (_selectedSlotDisplay)
            {
                //Visual
                _selectedSlotDisplay.UnhighlightSlot();
                //Logic
                _actionBar.OnDeSelectItem(_selectedSlotDisplay.SlotIndex);
                _selectedSlotDisplay = null;
            }
        }
        
        #region Keyboard Input

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectSlot(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectSlot(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SelectSlot(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SelectSlot(5); 
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SelectSlot(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SelectSlot(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SelectSlot(8);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SelectSlot(9);
            }
        }


        #endregion

        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            _actionBar.LeftClickItem(slotDisplay.SlotIndex);
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            //if the slot is selected, deselect it
            if(slotIndex == _actionBar.SelectedSlotIndex)
            {
                DeselectPreviousSlot();
            }
            return _actionBar.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            _actionBar.AddItemToEmptySlot(item, slotIndex);
            //Debug.Log("Item added to slot " + slotIndex + " in action bar." +_actionBar.SelectedSlotIndex);
            if(slotIndex == _actionBar.SelectedSlotIndex)
            {
                SelectSlot(slotIndex);
            }
        }
    }
}

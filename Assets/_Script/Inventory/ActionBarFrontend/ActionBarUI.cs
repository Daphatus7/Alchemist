using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.ActionBarFrontend
{
    public class ActionBarUI : MonoBehaviour, IPlayerInventoryHandler
    {
        private PlayerInventory.PlayerInventory _playerInventory;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;
        
        private InventorySlotDisplay[] _inventorySlotDisplays;
        
        private InventorySlotDisplay _selectedSlotDisplay;

        // private void OnEnable()
        // {
        //     _playerInventory.OnInventorySlotChanged += UpdateSlotUI;
        //     // Subscribe to full inventory updates if needed
        //     // _actionBar.OnInventoryChanged += UpdateAllSlotsUI;
        // }
        //
        private void OnDisable()
        {
            _playerInventory.OnInventorySlotChanged -= UpdateSlotUI;
            // Unsubscribe from full inventory updates if needed
            // _actionBar.OnInventoryChanged -= UpdateAllSlotsUI;
        }

        public void InitializeInventoryUI(PlayerInventory.PlayerInventory playerInventory, int width, int height, int selectedSlot = 0)
        {
            int capacity = width * height;

            _playerInventory = playerInventory;
            _inventorySlotDisplays = new InventorySlotDisplay[capacity];

            //Initialize the inventory slot Displays
            for(int i = 0; i < _playerInventory.SlotCount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlotDisplay inventorySlotDisplay = slot.GetComponent<InventorySlotDisplay>();
                _inventorySlotDisplays[i] = inventorySlotDisplay;
                // Initialize the slot
                inventorySlotDisplay.InitializeInventorySlot(this, i, _playerInventory.SlotType);
                inventorySlotDisplay.ClearSlot();
            }
            
            _playerInventory.OnInventorySlotChanged += UpdateSlotUI;
            SelectSlot(selectedSlot);
        }

        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventorySlotDisplays.Length)
                return;
            
            var itemStack = _playerInventory.GetItemStackAt(slotIndex);
            if(itemStack == null || itemStack.IsEmpty)
            {
                _inventorySlotDisplays[slotIndex].ClearSlot();
                return;
            }
            foreach (var position in itemStack.ItemPositions)
            {
                Debug.Log("Item position: " + position);
                var sIndex = position.x * _playerInventory.Height + position.y;
                _inventorySlotDisplays[sIndex].SetSlot(itemStack);
            }
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
            var itemStack = _playerInventory.GetItemStackAt(slotIndex);
            if(itemStack == null || itemStack.IsEmpty)
            {
                Debug.LogWarning("No item in slot.");
                //still deselect the previous item
                DeselectPreviousSlot();
                _selectedSlotDisplay = _inventorySlotDisplays[slotIndex];
                _selectedSlotDisplay?.HighlightSlot();
                
                _playerInventory.OnSelectNone();
                return;
            }
            
            //Case 3: Check if selecting the same slot and is not seed item
            if (_selectedSlotDisplay && _selectedSlotDisplay.SlotIndex == slotIndex && _playerInventory.GetItemStackAt(slotIndex).ItemData.ItemTypeString != "Seed")
            {
                // Use the selected slot
                _playerInventory.LeftClickItem(slotIndex);
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
            _playerInventory.OnSelectItem(slotIndex);
        }

        
        private void DeselectPreviousSlot()
        {
            if (_selectedSlotDisplay)
            {
                //Visual
                _selectedSlotDisplay.UnhighlightSlot();
                //Logic
                _playerInventory.OnDeSelectItem(_selectedSlotDisplay.SlotIndex);
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
            _playerInventory.LeftClickItem(slotDisplay.SlotIndex);
        }

        public ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            //if the slot is selected, deselect it
            if(slotIndex == _playerInventory.SelectedSlotIndex)
            {
                DeselectPreviousSlot();
            }
            return _playerInventory.RemoveAllItemsFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex)
        {
            _playerInventory.AddItemToEmptySlot(itemStack, slotIndex);
            //Debug.Log("Item added to slot " + slotIndex + " in action bar." +_actionBar.SelectedSlotIndex);
            if(slotIndex == _playerInventory.SelectedSlotIndex)
            {
                SelectSlot(slotIndex);
            }
        }

        public ItemStack AddItem(ItemStack itemStack)
        {
            return _playerInventory.AddItem(itemStack);
        }
        
        public bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }
        
        public bool CanFitItem(int targetSlotIndex, ItemStack comparingItemStack)
        {
            return _playerInventory.CanFitItem(targetSlotIndex, comparingItemStack);
        }

        public void AddGold(int amount)
        {
            _playerInventory.InventoryOwner.AddGold(amount);
        }

        public bool RemoveGold(int amount)
        {
            return _playerInventory.InventoryOwner.RemoveGold(amount);
        }
    }
}

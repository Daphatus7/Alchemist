using System;
using _Script.Character;
using _Script.Inventory.InventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.PlayerInventory
{
    public class PlayerInventory : PlayerContainer
    {
        private InventoryItem _selectedItem;
        private int _selectedSlotIndex;

        public PlayerInventory(PlayerCharacter owner, int capacity, int selectedSlotIndex = 0) : base(owner, capacity)
        {
            _selectedSlotIndex = selectedSlotIndex;
            _selectedItem = Slots[selectedSlotIndex];
            
            Debug.Log("By default, the player will select the first item when loaded.");
            OnSelectItem(selectedSlotIndex);
        }

        public int SelectedSlotIndex => _selectedSlotIndex;
        
        private ActionBarContext _actionBarContext;

        private void SetSelectedItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length)
            {
                _selectedSlotIndex = -1;
                _selectedItem = null;
            }
            else
            {
                _selectedSlotIndex = slotIndex;
                _selectedItem = Slots[slotIndex];
            }
        }

        /// <summary>
        /// Called by external classes to select the item at a given slot.
        /// Based on the item type, sets the appropriate strategy.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void OnSelectItem(int slotIndex)
        {
            SetSelectedItem(slotIndex);
            if (_selectedItem == null || _selectedItem.IsEmpty)
            {
                Debug.LogWarning("No item selected in that slot.");
                return;
            }

            var itemType = _selectedItem.ItemData.ItemTypeString;
            
            // Create context to handle item usage
            _actionBarContext = new ActionBarContext(LeftClickItem, RemoveWeaponOrTorchItem, slotIndex, _selectedItem.ItemData);

            // Now select strategy based on item type
            switch (itemType)
            {
                case "Seed":
                    // Use generic strategy for seeds
                    inventoryOwner.SetGenericStrategy();
                    inventoryOwner.GenericStrategy.ChangeItem(_actionBarContext);
                    break;
                
                case "Weapon":
                    // Use weapon strategy for weapons
                    inventoryOwner.SetWeaponStrategy();
                    inventoryOwner.WeaponStrategy.ChangeItem(_actionBarContext);
                    break;

                case "Torch":
                    // Use torch strategy for torches
                    inventoryOwner.SetTorchStrategy();
                    inventoryOwner.TorchStrategy.ChangeItem(_actionBarContext);
                    break;

                default:
                    // For any other item type, fallback to a generic strategy
                    Debug.LogWarning($"Item type '{itemType}' not recognized. Using GenericStrategy as fallback.");
                    inventoryOwner.SetGenericStrategy();
                    inventoryOwner.GenericStrategy.ChangeItem(_actionBarContext);
                    break;
            }
        }

        /// <summary>
        /// Called to indicate that no item is selected. 
        /// This does not remove strategies by itself but can be used before selecting another item.
        /// </summary>
        public void OnSelectNone()
        {
            _selectedItem = null;
            _selectedSlotIndex = -1;
        }

        /// <summary>
        /// Called by external classes to deselect the item at a given slot.
        /// Removes the associated strategy and item.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void OnDeSelectItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length || Slots[slotIndex].IsEmpty)
            {
                return;
            }
            RemoveStrategy(slotIndex);
        }

        private void RemoveStrategy(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length)
            {
                Debug.LogError($"Invalid slotIndex: {slotIndex}. Slots count: {Slots.Length}");
                return;
            }

            var itemTypeName = Slots[slotIndex].ItemData.ItemTypeString;
            Debug.Log($"Removing item from slotIndex: {slotIndex}, itemTypeName: {itemTypeName}");

            switch (itemTypeName)
            {
                case "Seed":
                    Debug.Log("Using GenericStrategy to remove Seed.");
                    inventoryOwner.GenericStrategy.RemoveItem();
                    break;

                case "Weapon":
                    Debug.Log("Using WeaponStrategy to remove Weapon.");
                    inventoryOwner.WeaponStrategy.RemoveItem();
                    break;

                case "Torch":
                    Debug.Log("Using TorchStrategy to remove Torch.");
                    inventoryOwner.TorchStrategy.RemoveItem();
                    break;

                default:
                    Debug.LogWarning($"Deselecting item of unrecognized type '{itemTypeName}'. Using GenericStrategy to remove.");
                    inventoryOwner.GenericStrategy.RemoveItem();
                    break;
            }

            // Unset the current strategy after removing the item from it
            Debug.Log("Unsetting current strategy.");
            inventoryOwner.UnsetStrategy();

            Debug.Log("Clearing selection.");
            _selectedItem = null;
            _selectedSlotIndex = -1;
        }

        
        protected override void OnItemUsedUp(int slotIndex)
        {
            // If the used up item is currently selected, we remove the associated strategy.
            if (slotIndex == _selectedSlotIndex)
            {
                _selectedItem = null;
                RemoveStrategy(slotIndex);
            }
        }
        
        /// <summary>
        /// Should Only be called for Weapons and Torches. 
        /// </summary>
        /// <param name="slotIndex"></param>
        public void RemoveWeaponOrTorchItem(int slotIndex)
        {
            RemoveAllItemsFromSlot(slotIndex);
        }
    }

    public class ActionBarContext
    {
        private readonly Action<int> _use; 
        private readonly Action<int> _remove;
        private readonly int _selectedSlotIndex;
        private readonly ItemData _itemData;

        public ItemData ItemData => _itemData;

        public ActionBarContext(Action<int> use,Action<int> remove,int selectedSlotIndex, ItemData itemData)
        {
            _use = use ?? throw new ArgumentNullException(nameof(use));
            _itemData = itemData ?? throw new ArgumentNullException(nameof(itemData));
            _remove = remove ?? throw new ArgumentNullException(nameof(remove));
            _selectedSlotIndex = selectedSlotIndex;
        }

        public void UseItem()
        {
            _use(_selectedSlotIndex);
        }
        
        public void RemoveWeaponOrTorch()
        {
            _remove(_selectedSlotIndex);
        }
    }
}

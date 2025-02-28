using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.ItemInstance;
using _Script.Inventory.SlotFrontend;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Items.Lootable;
using UnityEngine;

namespace _Script.Inventory.PlayerInventory
{
    public class PlayerInventory : PlayerContainer
    {
        
        private ItemInstance.ItemInstance _selectedItemInstance;
        private int _selectedSlotIndex;
        
        public override SlotType SlotType => SlotType.PlayerInventory;

        public PlayerInventory(PlayerCharacter owner, int width, int height, int selectedSlotIndex = 0) : base(owner, width, height)
        {
            _selectedSlotIndex = selectedSlotIndex;
            _selectedItemInstance = GetItemInstanceAt(selectedSlotIndex);
            OnSelectItem(selectedSlotIndex);
        }
        
        public PlayerInventory(PlayerCharacter owner, PlayerInventorySave inventorySave) : base(owner, inventorySave)
        {
            _selectedSlotIndex = inventorySave.selectedSlotIndex;
            _selectedItemInstance = GetItemInstanceAt(_selectedSlotIndex);
            OnSelectItem(_selectedSlotIndex);
        }

        public int SelectedSlotIndex
        {
            get => _selectedSlotIndex;
            set => _selectedSlotIndex = value;
        }

        private ActionBarContext _actionBarContext;

        private void SetSelectedItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                _selectedSlotIndex = -1;
                _selectedItemInstance = null;
            }
            else
            {
                _selectedSlotIndex = slotIndex;
                _selectedItemInstance = GetItemInstanceAt(slotIndex);
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
            
            if (_selectedItemInstance == null)
            {
                Debug.LogWarning("No item selected in that slot.");
                return;
            }

            var itemType = _selectedItemInstance.ItemTypeString;
            
            // Create context to handle item usage
            _actionBarContext = new ActionBarContext(OnUseSelectedItem, 
                RemoveWeaponOrTorchItem, 
                slotIndex, _selectedItemInstance);

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
                    Debug.Log("Setting Weapon Strategy");
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
                    OnSelectNone();
                    break;
            }
        }

        /// <summary>
        /// Called to indicate that no item is selected. 
        /// This does not remove strategies by itself but can be used before selecting another item.
        /// </summary>
        public void OnSelectNone()
        {
            _selectedItemInstance = null;
            _selectedSlotIndex = -1;
        }

        /// <summary>
        /// Called by external classes to deselect the item at a given slot.
        /// Removes the associated strategy and item.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void OnDeSelectItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                return;
            }
            if(GetItemInstanceAt(slotIndex) == null)
            {
                return;
            }
            RemoveStrategy(slotIndex);
        }

        private void RemoveStrategy(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                Debug.LogError($"Invalid slotIndex: {slotIndex}. Slots count: {SlotCount}");
                return;
            }

            var itemTypeName = GetItemInstanceAt(slotIndex).ItemTypeString;

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
                    Debug.Log($"Deselecting item of unrecognized type '{itemTypeName}'. Using GenericStrategy to remove.");
                    inventoryOwner.GenericStrategy.RemoveItem();
                    break;
            }

            // Unset the current strategy after removing the item from it
            inventoryOwner.UnsetStrategy();

            _selectedItemInstance = null;
            _selectedSlotIndex = -1;
        }

        
        protected override void OnItemUsedUp(int slotIndex)
        {
            // If the used up item is currently selected, we remove the associated strategy.
            if (slotIndex == _selectedSlotIndex)
            {
                _selectedItemInstance = null;
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

        public void OnUseSelectedItem(int slotIndex)
        {
            UseItem(slotIndex);
        }

        public override int GetItemCount(string itemItemID)
        {
            int count = 0;
            foreach(var stack in ItemInstances)
            {
                if(stack.ItemID == itemItemID)
                {
                    count += stack.Quantity;
                }
            }
            return count;
        }
        
        public void DropItem(int slotIndex)
        {
            //Check valid slot index
            if(slotIndex < 0 || slotIndex >= SlotCount)
            {
                Debug.LogError("Invalid slot index to drop item: " + slotIndex);
                return;
            }
            //check if there is an item in the slot
            var itemInstance = GetItemInstanceAt(slotIndex);
            if (itemInstance == null)
            {
                Debug.LogError("No item to drop in slot: " + slotIndex);
                return;
            }
            var removedItem = RemoveAllItemsFromSlot(slotIndex);
            //Drop the item
            ItemLootable.DropItem(inventoryOwner.transform.position, removedItem);
        }
        
        #region Save and load 

        public override InventorySave OnSaveData()
        {
            var save = new PlayerInventorySave
            {
                selectedSlotIndex = _selectedSlotIndex,
                inventoryUniqueID = UniqueID,
                items = new ItemSave[ItemInstances.Count],
                height = Height,
                width = Width
            };
            
            foreach(var instance in ItemInstances)
            {
                if(instance == null)
                {
                    throw new ArgumentNullException(nameof(instance) + " is null.");
                }
                var itemSave = instance.OnSaveData();
                save.items[ItemInstances.IndexOf(instance)] = itemSave;
            }
            
            return save;
        }

        #endregion
    }

    #region ActionBarContext

    public class ActionBarContext
    {
        private readonly Action<int> _use; 
        private readonly Action<int> _remove;
        private readonly int _selectedSlotIndex;
        private readonly ItemInstance.ItemInstance _itemInstance;

        public ItemInstance.ItemInstance ItemInstance => _itemInstance;

        public ActionBarContext(Action<int> use,Action<int> remove,int selectedSlotIndex, ItemInstance.ItemInstance itemInstance)
        {
            _use = use ?? throw new ArgumentNullException(nameof(use));
            _itemInstance = itemInstance ?? throw new ArgumentNullException(nameof(itemInstance));
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


    #endregion
    
    [Serializable]
    public class PlayerInventorySave : InventorySave
    {
        public int selectedSlotIndex;
    }
}

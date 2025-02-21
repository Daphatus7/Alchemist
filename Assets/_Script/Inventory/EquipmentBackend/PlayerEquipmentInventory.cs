using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Inventory.InventoryBackend;
using _Script.Items;
using _Script.Items.AbstractItemTypes;
using UnityEngine;

namespace _Script.Inventory.EquipmentBackend
{
    public sealed class PlayerEquipmentInventory : MonoBehaviour
    {

        private Dictionary<PlayerEquipmentSlotType, ItemInstance.ItemInstance> _equipmentSlots;
        private PlayerCharacter _playerCharacter;
        
        //OnEquipmentChanged event
        public event Action OnEquipmentChanged;
        
        
        private void Awake()
        {
            _playerCharacter = GetComponent<PlayerCharacter>();
            _equipmentSlots = new Dictionary<PlayerEquipmentSlotType, ItemInstance.ItemInstance>();
        }
        
        
        /**
         * Equip an item to the player
         * 1. If there is an item in the slot, return the item to the inventory
         * 2. If there is no item in the slot, equip the item qnd return null
         */
        private ItemInstance.ItemInstance EquipItem(EquipmentItem item)
        {
            var targetSlot = ConvertToPlayerEquipmentInventory(item);
            //remove the effect of the equipped item

            //if there is an item in the slot, return the item to the inventory
            ItemInstance.ItemInstance tempSlotItemInstance = null;
            
            //get the item in the slot, return the item to the inventory
            if (_equipmentSlots.TryGetValue(targetSlot, out var slot))
            {
                //get the item in the slot, return the item to the inventory
                tempSlotItemInstance = slot;
                //equip the item
            }
            
            //remove the effect of the old item if any
            //apply the effect of the new item
            switch (targetSlot)
            {
                case PlayerEquipmentSlotType.Chest:
                    EquipArmour(item);
                    break;
                case PlayerEquipmentSlotType.Accessory:
                    EquipAccessory(item);
                    break;
                case PlayerEquipmentSlotType.Head:
                default:
                    Debug.LogError("Invalid equipment type");
                    break;
            }
            
            throw new NotImplementedException("Implement the equip item method");
            //_equipmentSlots[targetSlot] = new ItemStack(pivotPosition, item, 1);
            
            OnOnEquipmentChanged();
            return tempSlotItemInstance;
        }


        public ItemInstance.ItemInstance RemoveEquipmentFromSlot(int slotIndex)
        {
            var slot = (PlayerEquipmentSlotType) slotIndex;
            ItemInstance.ItemInstance tempSlotItemInstance = null; //UnEquip the item
            if (_equipmentSlots.TryGetValue(slot, out var slotItem))
            {
                //remove the effect of the equipped item
                if(slot == PlayerEquipmentSlotType.Chest)
                {
                    UnequipArmour();
                }
                else if(slot == PlayerEquipmentSlotType.Accessory)
                {
                    UnequipAccessory();
                }
                else
                {
                    Debug.LogError("Invalid equipment type");
                }

                tempSlotItemInstance = _equipmentSlots[slot];
                _equipmentSlots[slot] = null;
                OnOnEquipmentChanged();
            }
            return tempSlotItemInstance;
        }
        
        
        public void UnequipItem(int slotIndex)
        {
            var item = RemoveEquipmentFromSlot(slotIndex);
            if (item != null)
            {
                //return the item to the inventory
               // _playerCharacter.PlayerContainer.AddItem(item);
            }
            
        }

        
        private void EquipArmour(EquipmentItem item)
        {
            //remove the effect of the equipped item
            //get leaf item
            //if there is an item in the slot, return the item to the inventory
            //equip the item
        }
        
        private void EquipAccessory(EquipmentItem item)
        {
            //remove the effect of the equipped item
            //get leaf item
            //if there is an item in the slot, return the item to the inventory
            //equip the item
        }
        
        private void UnequipArmour()
        {
            //remove the effect of the equipped item
            //get leaf item
            //if there is an item in the slot, return the item to the inventory
            //equip the item
        }
        
        private void UnequipAccessory()
        {
            //remove the effect of the equipped item
            //get leaf item
            //if there is an item in the slot, return the item to the inventory
            //equip the item
        }
        
        public ItemInstance.ItemInstance GetEquipment(PlayerEquipmentSlotType slot)
        {
            return _equipmentSlots.GetValueOrDefault(slot);
        }
        
        private PlayerEquipmentSlotType ConvertToPlayerEquipmentInventory(in EquipmentItem equipmentItem)
        {
            
            switch (equipmentItem.EquipmentType)
            {
                case EquipmentType.Accessory:
                    return PlayerEquipmentSlotType.Accessory;
                case EquipmentType.Armour:
                    return PlayerEquipmentSlotType.Chest;
                case EquipmentType.Weapon:
                    return PlayerEquipmentSlotType.Weapon;
                default:
                    //exception error
                    break;
            }
            return PlayerEquipmentSlotType.Head;
        }

        private void OnOnEquipmentChanged()
        {
            OnEquipmentChanged?.Invoke();
        }
        
        public ItemInstance.ItemInstance Handle_Equip(EquipmentItem equipmentItem)
        {
            return EquipItem(equipmentItem);
        }
        
    }
    
    public enum PlayerEquipmentSlotType
    {
        Head,
        Chest,
        Weapon,
        Accessory,
    }
    
}
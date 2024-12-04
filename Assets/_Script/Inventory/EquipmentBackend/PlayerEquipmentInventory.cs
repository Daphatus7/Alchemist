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

        private Dictionary<PlayerEquipmentSlotType, InventoryItem> _equipmentSlots;
        private PlayerCharacter _playerCharacter;
        
        //OnEquipmentChanged event
        public event Action OnEquipmentChanged;
        
        
        private void Awake()
        {
            _playerCharacter = GetComponent<PlayerCharacter>();
            _equipmentSlots = new Dictionary<PlayerEquipmentSlotType, InventoryItem>();
        }
        
        
        /**
         * Equip an item to the player
         * 1. If there is an item in the slot, return the item to the inventory
         * 2. If there is no item in the slot, equip the item qnd return null
         */
        private InventoryItem EquipItem(EquipmentItem item)
        {
            var targetSlot = ConvertToPlayerEquipmentInventory(item);
            //remove the effect of the equipped item

            //if there is an item in the slot, return the item to the inventory
            InventoryItem tempSlotItem = null;
            
            //get the item in the slot, return the item to the inventory
            if (_equipmentSlots.TryGetValue(targetSlot, out var slot))
            {
                //get the item in the slot, return the item to the inventory
                tempSlotItem = slot;
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
            

            _equipmentSlots[targetSlot] = new InventoryItem(item, 1);
            
            OnOnEquipmentChanged();
            return tempSlotItem;
        }


        public InventoryItem RemoveEquipmentFromSlot(int slotIndex)
        {
            var slot = (PlayerEquipmentSlotType) slotIndex;
            InventoryItem tempSlotItem = null; //UnEquip the item
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

                tempSlotItem = _equipmentSlots[slot];
                _equipmentSlots[slot] = null;
                OnOnEquipmentChanged();
            }
            return tempSlotItem;
        }
        
        
        public void UnequipItem(int slotIndex)
        {
            var item = RemoveEquipmentFromSlot(slotIndex);
            if (item != null)
            {
                //return the item to the inventory
                _playerCharacter.PlayerInventory.AddItem(item);
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
        
        public InventoryItem GetEquipment(PlayerEquipmentSlotType slot)
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
        
        public InventoryItem Handle_Equip(EquipmentItem equipmentItem)
        {
            return EquipItem(equipmentItem);
        }

        public InventoryItem Handle_Unequip_RemoveEffect(InventoryItem fromSlot)
        {
            return null;
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
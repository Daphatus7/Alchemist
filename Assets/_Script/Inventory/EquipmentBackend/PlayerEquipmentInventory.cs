using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.EquipmentBackend
{
    public sealed class PlayerEquipmentInventory : MonoBehaviour, IPlayerEquipmentHandle
    {

        private Dictionary<PlayerEquipmentSlotType, InventoryItem> _equipmentSlots;
        
        //OnEquipmentChanged event
        public event Action OnEquipmentChanged;
        
        
        private void Awake()
        {
            _equipmentSlots = new Dictionary<PlayerEquipmentSlotType, InventoryItem>();
        }
        
        
        /**
         * Equip an item to the player
         * 1. If there is an item in the slot, return the item to the inventory
         * 2. If there is no item in the slot, equip the item qnd return null
         */
        private InventoryItem EquipItem(InventoryItem item)
        {
            InventoryItem tempSlotItem = null;
            PlayerEquipmentSlotType targetSlot = ConvertToPlayerEquipmentInventory(item.ItemData as EquipmentItem);
            if (_equipmentSlots.TryGetValue(targetSlot, out var slot))
            {
                //get the item in the slot, return the item to the inventory
                tempSlotItem = slot;
                //equip the item
                _equipmentSlots[targetSlot] = item;
                return tempSlotItem;
            }
            _equipmentSlots[targetSlot] = item;
            
            OnOnEquipmentChanged();
            return null;
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
        
        public InventoryItem Handle_EquipItem(InventoryItem item)
        {
            return EquipItem(item);
        }

        public InventoryItem Handle_UnequipItem(InventorySlot fromSlot)
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
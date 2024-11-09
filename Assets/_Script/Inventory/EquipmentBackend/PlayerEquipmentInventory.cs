using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Items;
using _Script.Items.AbstractItemTypes;
using UnityEngine;

namespace _Script.Inventory.EquipmentBackend
{
    public class PlayerEquipmentInventory : MonoBehaviour, IPlayerEquipmentHandle
    {

        private Dictionary<PlayerEquipmentSlot, InventoryItem> _equipmentSlots;
        
        
        /**
         * Equip an item to the player
         * 1. If there is an item in the slot, return the item to the inventory
         * 2. If there is no item in the slot, equip the item qnd return null
         */
        public InventoryItem EquipItem(InventoryItem item)
        {
            InventoryItem tempSlot = null;
            PlayerEquipmentSlot targetSlot = ConvertToPlayerEquipmentInventory(item.ItemData as EquipmentItem);
            if (_equipmentSlots.TryGetValue(targetSlot, out var slot))
            {
                tempSlot = slot;
                
            }
            return null;
        }

        private PlayerEquipmentSlot ConvertToPlayerEquipmentInventory(in EquipmentItem equipmentItem)
        {
            switch (equipmentItem.EquipmentType)
            {
                // case EquipmentItem.EquipmentType.Head:
                //     return PlayerEquipmentSlot.Head;
                // case EquipmentItem.Chest:
                //     return PlayerEquipmentSlot.Chest;
                // case EquipmentItem.Weapon:
                //     return PlayerEquipmentSlot.Weapon;
                // default:
                //     return PlayerEquipmentSlot.Head;
            }
            return PlayerEquipmentSlot.Head;
        }
        
    }
    
    public enum PlayerEquipmentSlot
    {
        Head,
        Chest,
        Weapon
    }
    
}
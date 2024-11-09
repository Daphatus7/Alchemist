using System.Collections.Generic;
using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Items;
using _Script.Items.AbstractItemTypes;
using UnityEngine;

namespace _Script.Inventory.EquipmentFrontend
{
    public class EquipmentUI : MonoBehaviour
    {
        private InventorySlot _headSlot;
        private InventorySlot _chestSlot;
        private InventorySlot _weaponSlot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory;
        
    }
}
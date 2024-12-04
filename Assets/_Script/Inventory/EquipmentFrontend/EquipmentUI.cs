using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.EquipmentFrontend
{
    public class EquipmentUI : MonoBehaviour, IEquipmentUIHandle
    {
        private InventorySlotDisplay _headSlot;
        private InventorySlotDisplay _chestSlot;
        private InventorySlotDisplay _weaponSlot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory;
        [SerializeField] private GameObject equipmentPanel;

        private void Awake()
        {
            _headSlot = Instantiate(slotPrefab, equipmentPanel.transform).GetComponent<InventorySlotDisplay>();
            _chestSlot = Instantiate(slotPrefab, equipmentPanel.transform).GetComponent<InventorySlotDisplay>();
            _weaponSlot = Instantiate(slotPrefab, equipmentPanel.transform).GetComponent<InventorySlotDisplay>();
            
            _headSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Head, SlotType.Equipment);
            _chestSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Chest, SlotType.Equipment);
            _weaponSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Weapon, SlotType.Equipment);
        }
        
        private void OnEnable()
        {
            playerEquipmentInventory.OnEquipmentChanged += UpdateUI;
            UpdateUI();
        }

        // private void OnEnable()
        //show the equipment ui
        
        private void UpdateUI()
        {
            _headSlot.SetSlot(playerEquipmentInventory.GetEquipment(PlayerEquipmentSlotType.Head));
            _chestSlot.SetSlot(playerEquipmentInventory.GetEquipment(PlayerEquipmentSlotType.Chest));
            _weaponSlot.SetSlot(playerEquipmentInventory.GetEquipment(PlayerEquipmentSlotType.Weapon));
        }
        
        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            playerEquipmentInventory.UnequipItem(slotDisplay.SlotIndex);
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            return playerEquipmentInventory.RemoveEquipmentFromSlot(slotIndex);
        }

        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            //check if is equipment item
            if(item.ItemData is EquipmentItem equipmentItem)
                playerEquipmentInventory.Handle_Equip(equipmentItem);
            Debug.LogError("This is not an equipment item");
        }

        public InventoryItem AddItem(InventoryItem item)
        {
            //check if is equipment item
            if(item.ItemData is EquipmentItem equipmentItem)
                playerEquipmentInventory.Handle_Equip(equipmentItem);
            Debug.LogError("This is not an equipment item");
            return null;
        }
        
        public bool AcceptsItem(InventoryItem item)
        {
            return item.ItemData is EquipmentItem;
        }
    }
}
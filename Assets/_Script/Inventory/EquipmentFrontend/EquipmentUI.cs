using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.SlotFrontend;
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
            
            _headSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Head);
            _chestSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Chest);
            _weaponSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Weapon);
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
    }
}
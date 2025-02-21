// using System.Collections.Generic;
// using _Script.Inventory.EquipmentBackend;
// using _Script.Inventory.InventoryBackend;
// using _Script.Inventory.InventoryFrontend;
// using _Script.Inventory.SlotFrontend;
// using _Script.Items;
// using UnityEngine;
//
// namespace _Script.Inventory.EquipmentFrontend
// {
//     public class EquipmentUI : MonoBehaviour, IEquipmentUIHandle
//     {
//         private InventorySlotInteraction _headSlot;
//         private InventorySlotInteraction _chestSlot;
//         private InventorySlotInteraction _weaponSlot;
//         [SerializeField] private GameObject slotPrefab;
//         [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory;
//         [SerializeField] private GameObject equipmentPanel;
//
//         private void Awake()
//         {
//             _headSlot = Instantiate(slotPrefab, equipmentPanel.transform).GetComponent<InventorySlotInteraction>();
//             _chestSlot = Instantiate(slotPrefab, equipmentPanel.transform).GetComponent<InventorySlotInteraction>();
//             _weaponSlot = Instantiate(slotPrefab, equipmentPanel.transform).GetComponent<InventorySlotInteraction>();
//             
//             _headSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Head, SlotType.Equipment);
//             _chestSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Chest, SlotType.Equipment);
//             _weaponSlot.InitializeInventorySlot(this, (int)PlayerEquipmentSlotType.Weapon, SlotType.Equipment);
//         }
//         
//         private void OnEnable()
//         {
//             playerEquipmentInventory.OnEquipmentChanged += UpdateUI;
//             UpdateUI();
//         }
//
//         // private void OnEnable()
//         //show the equipment ui
//         
//         private void UpdateUI()
//         {
//             _headSlot.SetSlot(playerEquipmentInventory.GetEquipment(PlayerEquipmentSlotType.Head));
//             _chestSlot.SetSlot(playerEquipmentInventory.GetEquipment(PlayerEquipmentSlotType.Chest));
//             _weaponSlot.SetSlot(playerEquipmentInventory.GetEquipment(PlayerEquipmentSlotType.Weapon));
//         }
//         
//         public void OnSlotClicked(InventorySlotInteraction slotInteraction)
//         {
//             playerEquipmentInventory.UnequipItem(slotInteraction.SlotIndex);
//         }
//
//         public ItemInstance.ItemInstance RemoveAllItemsFromSlot(int slotIndex)
//         {
//             return playerEquipmentInventory.RemoveEquipmentFromSlot(slotIndex);
//         }
//
//         public void AddItemToEmptySlot(ItemInstance.ItemInstance itemInstance, List<Vector2Int> projectedPositions)
//         {
//             //check if is equipment item
//             if(itemInstance.ItemData is EquipmentItem equipmentItem)
//                 playerEquipmentInventory.Handle_Equip(equipmentItem);
//             Debug.LogError("This is not an equipment item");
//         }
//
//         public ItemInstance.ItemInstance AddItem(ItemInstance.ItemInstance itemInstance)
//         {
//             //check if is equipment item
//             if(itemInstance.ItemData is EquipmentItem equipmentItem)
//                 playerEquipmentInventory.Handle_Equip(equipmentItem);
//             Debug.LogError("This is not an equipment item");
//             return null;
//         }
//         
//         public bool AcceptsItem(ItemInstance.ItemInstance itemInstance)
//         {
//             return itemInstance.ItemData is EquipmentItem;
//         }
//
//         public bool CanFitItem(List<Vector2Int> projectedPositions)
//         {
//             return false;
//         }
//         
//         public Vector2Int GetSlotPosition(int slotIndex)
//         {
//             return Vector2Int.zero;
//         }
//
//         public int GetSlotIndex(Vector2Int position)
//         {
//             return 0;
//         }
//
//         public int GetItemsCount(int shiftedPivotIndex, List<Vector2Int> peakItemStack, out int onlyItemIndex)
//         {
//             onlyItemIndex = -1;
//             return 0;
//         }
//     }
// }
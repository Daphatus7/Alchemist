// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 59

using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontend
{
    public class InventoryManager : MonoBehaviour, IInventoryManagerHandler
    {
        [SerializeField] private List<InventoryUI> inventoryUIs = new List<InventoryUI>();
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private int initialPoolCount = 100;

        private Transform _poolParent;

        private void Awake()
        {
            // Create a parent for pooled slots
            GameObject poolParentObj = new GameObject("SlotPoolParent");
            poolParentObj.transform.SetParent(transform);
            _poolParent = poolParentObj.transform;

            // Initialize slot pool
            SlotPool.Initialize(slotPrefab, _poolParent, initialPoolCount);
        }

        public void LoadPlayerContainer(PlayerContainer playerContainer)
        {
            InventoryUI newInventoryUI = Instantiate(inventoryPanel, transform).GetComponent<InventoryUI>();
            newInventoryUI.InitializeInventoryUI(this, playerContainer);
            inventoryUIs.Add(newInventoryUI);
        }
        
        public void OnCloseInventory(InventoryUI inventoryUI)
        {
            inventoryUI.CloseInventoryUI();
        }

        public void OnCloseInventoryUI(InventoryUI inventoryUI)
        {
            inventoryUIs.Remove(inventoryUI);
        }
    }
    public interface IInventoryManagerHandler
    {
        void OnCloseInventoryUI(InventoryUI inventoryUI);
    }
}


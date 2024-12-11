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
        [SerializeField] private GameObject inventoryManagerPanel;
        [SerializeField] private GameObject inventoryPanelPrefab;

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

        public void TogglePlayerContainer(PlayerContainer playerContainer)
        {
            // Check if the container is already open
            foreach (var t in inventoryUIs)
            {
                if (t.CurrentContainer == playerContainer)
                {
                    // It's already open, so close it
                    OnCloseInventory(t);
                    return;
                }
            }

            // If not found, create a new UI for this container
            CreateInventoryUI(playerContainer);
        }
        
        private void CreateInventoryUI(PlayerContainer playerContainer)
        {
            InventoryUI newInventoryUI = Instantiate(inventoryPanelPrefab, inventoryManagerPanel.transform).GetComponent<InventoryUI>();
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


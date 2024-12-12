using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontend
{
    /// <summary>
    /// The InventoryManager is used to manage multiple bag systems, similar to WoW.
    /// It can open/close the player's main inventory and additional extra bags.
    /// </summary>
    public class InventoryManager : MonoBehaviour, IInventoryManagerHandler
    {
        [SerializeField] private GameObject inventoryManagerPanel;
        [SerializeField] private GameObject inventoryPanelPrefab;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private int initialPoolCount = 100;

        private Transform _poolParent;
        private bool _initialized;

        /// <summary>
        /// A dictionary of currently opened inventory UIs, keyed by the container's UniqueID.
        /// It can contain the main inventory UI and additional bag UIs simultaneously.
        /// </summary>
        private readonly Dictionary<string, InventoryUI> _openInventoryUIs = new Dictionary<string, InventoryUI>();

        // Temporary reference to store the newly created UI before we add it to the dictionary
        private InventoryUI _lastCreatedUIReference;

        private void Awake()
        {
            // Create a parent object to store pooled slot objects
            GameObject poolParentObj = new GameObject("SlotPoolParent");
            poolParentObj.transform.SetParent(transform);
            _poolParent = poolParentObj.transform;

            // Initialize the slot pool
            SlotPool.Initialize(slotPrefab, _poolParent, initialPoolCount);
            _initialized = true;
        }

        /// <summary>
        /// Toggles the display of a specified container (bag) by using its UniqueID.
        /// If it is already open, it will be closed; if not, a new UI will be created.
        /// This applies to the player's main inventory or additional bags.
        /// </summary>
        public void ToggleContainer(PlayerContainer container)
        {
            CreateInventoryUI(container);
return;
            string containerID = container.UniqueID;
            if (_openInventoryUIs.TryGetValue(containerID, out var ui))
            {
                // If already open, close the UI
                OnCloseInventory(ui);
            }
            else
            {
                // Not open yet, create a new UI
                CreateInventoryUI(container);
                // After CreateInventoryUI, _lastCreatedUIReference should hold the new UI
                _openInventoryUIs[containerID] = _lastCreatedUIReference;
            }
        }
        
        private void CreateInventoryUI(InventoryBackend.Inventory container)
        {
            InventoryUI newInventoryUI = Instantiate(inventoryPanelPrefab, inventoryManagerPanel.transform)
                .GetComponent<InventoryUI>();
            newInventoryUI.InitializeInventoryUI(this, container as PlayerContainer);
            
            // Store the newly created UI in the temporary field
            _lastCreatedUIReference = newInventoryUI;
        }

        /// <summary>
        /// Closes all opened bags.
        /// </summary>
        public void CloseAllBags()
        {
            // Be careful when modifying a dictionary during iteration.
            // Here we copy the values first.
            var openUIs = new List<InventoryUI>(_openInventoryUIs.Values);
            foreach (var ui in openUIs)
            {
                OnCloseInventory(ui);
            }
        }

        /// <summary>
        /// Called when a specific bag UI requests to be closed.
        /// Calls the UI's CloseInventoryUI method.
        /// </summary>
        public void OnCloseInventory(InventoryUI inventoryUI)
        {
            inventoryUI.CloseInventoryUI();
        }

        /// <summary>
        /// Called after the UI is fully closed. Removes the UI's associated container from the dictionary.
        /// </summary>
        public void OnCloseInventoryUI(InventoryUI inventoryUI)
        {
            // Find the corresponding container ID
            string containerID = null;
            foreach (var kvp in _openInventoryUIs)
            {
                if (kvp.Value == inventoryUI)
                {
                    containerID = kvp.Key;
                    break;
                }
            }

            if (containerID != null)
            {
                _openInventoryUIs.Remove(containerID);
            }
        }
    }

    public interface IInventoryManagerHandler
    {
        void OnCloseInventoryUI(InventoryUI inventoryUI);
    }
}
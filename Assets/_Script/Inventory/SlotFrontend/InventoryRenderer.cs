// Author : Peiyu Wang @ Daphatus
// 03 01 2025 01 51


using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryFrontendBase;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Script.Inventory.SlotFrontend
{
    public class InventoryRenderer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The inventory data to render (assign at runtime or in inspector).")]
        private InventoryBackend.Inventory _inventory;

        [Tooltip("Prefab for each slot's UI. Could contain an Image/Text, etc.")]
        public GameObject slotPrefab;

        [Tooltip("Parent transform where the slot UIs will be placed.")]
        public Transform slotsParent;

        // A local cache: slotIndex -> the UI GameObject we created
        private readonly List<InventorySlotDisplay> _slotUIs = new List<InventorySlotDisplay>();

        private void Awake()
        {
            var inv = GetComponent<InventoryUIBase<InventoryBackend.Inventory>>();
            
        }

        private void Start()
        {
            // Create all slots at start
            CreateAllSlots();
        }
        
        private void OnEnable()
        {
            if (_inventory == null)
            {
                _inventory = GetComponent<InventoryBackend.Inventory>();
            }
            if (_inventory != null)
            {
                _inventory.OnItemStackChanged += UpdateItemStacks;
            }
            else
            {
                throw new NullReferenceException("InventoryRenderer: Inventory is not assigned.");
            }
        }
        
        private void OnDestroy()
        {
            // unsubscribe to avoid leaks
            if (_inventory != null)
            {
                _inventory.OnItemStackChanged -= UpdateItemStacks;
            }
        }

        /// <summary>
        /// Creates UI objects for all slots in the inventory.
        /// Typically called once at start or inventory re-init.
        /// </summary>
        private void CreateAllSlots()
        {
            // Clean up old if any
            foreach (var kv in _slotUIs)
            {
                Destroy(kv);
            }
            _slotUIs.Clear();

            UpdateItemStacks();
        }

        private void UpdateItemStacks()
        {
            foreach(var item in _inventory.ItemStacks)
            {
                var newItemDisplay = Instantiate(slotPrefab, slotsParent);
                //May need to update location later
                var slotUI = newItemDisplay.GetComponent<InventorySlotDisplay>();
                slotUI.SetSlotImage(item.ItemData.ItemSprite);
                _slotUIs.Add(slotUI);
            }
        }
    }
}
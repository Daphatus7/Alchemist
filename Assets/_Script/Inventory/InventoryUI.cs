using System;
using _Script.Items;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Inventory
{
    
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private Items.Inventory playerInventory;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;
        
        void Start()
        {
            UpdateInventoryUI();
        }
        
        private void UpdateInventoryUI()
        {
            foreach (Transform child in inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < playerInventory.Capacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
                InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
                if (i < playerInventory.Items.Count)
                {
                    inventorySlot.SetSlot(playerInventory.Items[i]);
                }
                else
                {
                    inventorySlot.ClearSlot();
                }
            }
        }
    }

}
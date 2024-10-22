using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Script.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText; // Changed to TextMeshProUGUI
        [SerializeField] private Button slotButton;

        private void Awake()
        {

        }

        private void OnEnable()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
            }
        }
        
        private void OnDisable()
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveListener(OnSlotClicked);
            }
        }


        public void SetSlot(Items.InventoryItem item)
        {
            // Set icon and quantity
            if (item != null && item.Icon != null)
            {
                icon.sprite = item.Icon;
                icon.color = Color.white; // Make the icon fully visible
                quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : ""; // Only show quantity if more than 1
            }
            else
            {
                ClearSlot(); // Clear slot if item is null
            }
        }
        
        public void ClearSlot()
        {
            // Clear the icon and hide the quantity
            icon.sprite = null;
            icon.color = new Color(1, 1, 1, 0); // Fully transparent
            quantityText.text = "";
        }

        public void OnSlotClicked()
        {
            // Handle button click event, this can be expanded to interact with the item
            Debug.Log("Slot clicked!");
        }
    }
}
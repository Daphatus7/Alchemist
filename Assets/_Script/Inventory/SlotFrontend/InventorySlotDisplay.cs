using _Script.Inventory.InventoryFrontend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay: MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText; // Changed to TextMeshProUGUI
        [SerializeField] private Button slotButton;
        private IContainerUIHandle _inventoryUI;
        private int _slotIndex; public int SlotIndex => _slotIndex;
        
        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
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
        
        public virtual void ClearSlot()
        {
            // Clear the icon and hide the quantity
            icon.sprite = null;
            icon.color = new Color(1, 1, 1, 0); // Fully transparent
            quantityText.text = "";
        }

        public virtual void OnSlotClicked()
        {
            _inventoryUI.OnSlotClicked(this);
        }
    }
}
// Author : Peiyu Wang @ Daphatus
// 03 01 2025 01 23

using _Script.Items.AbstractItemTypes._Script.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay : MonoBehaviour
    {
        [SerializeField] private Image _slotImage;
        [SerializeField] private TextMeshProUGUI _slotText;
        
        
        public void SetDisplay(ItemData itemData, int amount)
        {
            _slotImage.sprite = itemData.itemIcon;
            _slotText.text = amount.ToString();
        }
    }
}
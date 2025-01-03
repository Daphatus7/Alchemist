// Author : Peiyu Wang @ Daphatus
// 03 01 2025 01 23

using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay : MonoBehaviour
    {
        private Image _slotImage;
        private TextMeshProUGUI _slotText;
        
        private void Awake()
        {
            _slotImage = GetComponent<Image>();
            _slotText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        public void SetSlotImage(Sprite sprite)
        {
            _slotImage.sprite = sprite;
        }
        
        
    }
}
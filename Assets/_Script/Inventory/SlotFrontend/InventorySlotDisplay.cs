// Author : Peiyu Wang @ Daphatus
// 03 01 2025 01 23

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay : MonoBehaviour
    {
        [SerializeField] private Image _slotImage;
        [SerializeField] private TextMeshProUGUI _slotText;
        
        public void SetSlotImage(Sprite sprite)
        {
            _slotImage.sprite = sprite;
        }
    }
}
// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 23

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{
    public class AlchemyRecipeDisplayUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        
        public void SetDisplay(Sprite sprite, string itemName)
        {
            icon.sprite = sprite;
            nameText.text = itemName;
        }
    }
}
// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 02

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{
    public class AlchemyReagentDisplayUI : MonoBehaviour
    {
        [SerializeField] private Image reagentIcon;
        [SerializeField] private TextMeshProUGUI reagentNameText;
        [SerializeField] private TextMeshProUGUI reagentAmountText;
        
        public void SetDisplay(Sprite icon, string reagentName, int amount)
        {
            reagentIcon.sprite = icon;
            reagentNameText.text = reagentName;
            reagentAmountText.text = amount.ToString();
        }
    }
}
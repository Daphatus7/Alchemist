// Author : Peiyu Wang @ Daphatus
// 07 01 2025 01 01

using _Script.Items.AbstractItemTypes._Script.Items;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.UserInterface
{
    public class ItemDetail : Singleton<ItemDetail>, IUIHandler
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemRarity;
        [SerializeField] private TextMeshProUGUI itemDescription;
        [SerializeField] private TextMeshProUGUI itemType;
        [SerializeField] private TextMeshProUGUI itemValue;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        public void Start()
        {
            HideUI();
        }
        
        public void ShowItemDetail(ItemData itemData)
        {
            itemName.text = itemData.itemName;
            itemRarity.text = itemData.rarity.ToString();
            itemDescription.text = itemData.itemDescription;
            itemType.text = itemData.ItemTypeString;
            itemValue.text = itemData.Value.ToString();
        }
        

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }
    }
}
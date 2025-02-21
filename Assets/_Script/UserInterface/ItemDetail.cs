// Author : Peiyu Wang @ Daphatus
// 07 01 2025 01 01

using _Script.Inventory.ItemInstance;
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
        public void Start()
        {
            HideUI();
        }
        
        public void ShowItemDetail(ItemInstance itemInstance)
        {
            itemName.text = itemInstance.ItemName;
            itemRarity.text = itemInstance.Rarity.ToString();
            itemDescription.text = itemInstance.ItemDescription;
            itemType.text = itemInstance.ItemTypeString;
            itemValue.text = itemInstance.Value.ToString();
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
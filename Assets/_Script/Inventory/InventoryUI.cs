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

        public void UpdateInventoryUI()
        {
        }
    }

}
// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 34

using _Script.Inventory.InventoryFrontendBase;
using _Script.Inventory.MerchantInventoryBackend;
using _Script.NPC.NpcBackend;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

namespace _Script.Inventory.MerchantInventoryFrontend
{
    public class MerchantInventoryUI : InventoryUIBase<MerchantInventory>, IMerchantInventoryService
    {
        public NpcHandlerType HandlerName => NpcHandlerType.Merchant;
        
        private void Awake()
        {
            ServiceLocator.Instance.Register<IMerchantInventoryService>(this);
        }
        
        private void OnDestroy()
        {
            ServiceLocator.Instance.Unregister<IMerchantInventoryService>();
        }        
        
        public void LoadMerchantInventory(MerchantInventory merchantInventory)
        {
            ShowInventory();
            AssignInventory(merchantInventory);
            InitializeInventoryUI();
        }

        public void CloseMerchantInventory()
        {
            ClearInventory();
            HideInventory();
        }
    }
}
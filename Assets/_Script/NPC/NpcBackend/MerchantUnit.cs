// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 56

using System;
using System.Collections.Generic;
using System.Linq;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.MerchantInventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Managers;
using _Script.Managers.GlobalUpdater;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [DefaultExecutionOrder(500)]
    public class MerchantUnit : MonoBehaviour, INpcHandler, IGlobalUpdate
    {
        
        private MerchantInventory _merchantInventory;
        [SerializeField] private List<ItemData> itemsForSale;
        
        private void Awake()
        {
            InitializeMerchantInventory();
        }

        private void OnEnable()
        {
            GameManager.Instance.RegisterGlobalUpdater(this);
        }

        private void OnDisable()
        {
            GameManager.Instance.UnregisterGlobalUpdater(this);
        }

        public void InitializeMerchantInventory()
        {
            //Add merchant inventory to merchant
            var itemsToAdd = new List<ItemStack>();
            foreach(var item in itemsForSale)
            {
                itemsToAdd.Add(new ItemStack(item, item.MaxStackSize));
            }
            _merchantInventory = new MerchantInventory(20, itemsToAdd);
        }
        
        public void LoadNpcModule()
        {
            ServiceLocator.Instance.Get<IMerchantInventoryService>().LoadMerchantInventory(_merchantInventory);
        }

        public void UnloadNpcModule()
        {
            ServiceLocator.Instance.Get<IMerchantInventoryService>().CloseMerchantInventory();
        }

        public NpcHandlerType HandlerType => NpcHandlerType.Merchant;
        public void Refresh()
        {
            //Refresh merchant inventory
            InitializeMerchantInventory();
        }
    }
}
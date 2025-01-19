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
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [DefaultExecutionOrder(500)]
    public class MerchantUnit : MonoBehaviour, INpcHandler, IGlobalUpdate
    {
        
        private Npc _npc;
        private MerchantInventory _merchantInventory;
        [SerializeField] private List<ItemData> itemsForSale;
        [SerializeField] private int inventoryWidth = 5;
        [SerializeField] private int inventoryHeight = 5;
        
        private void Awake()
        {
            _npc = GetComponent<Npc>();
            InitializeMerchantInventory();
        }

        private void OnEnable()
        {
            GameManager.Instance.RegisterGlobalUpdater(this);
        }

        private void OnDisable()
        {
            if(GameManager.Instance != null)
                GameManager.Instance.UnregisterGlobalUpdater(this);
        }
        
        public void InitializeMerchantInventory()
        {
            //Add merchant inventory to merchant
            var itemsToAdd = new List<ItemStack>();
            foreach(var item in itemsForSale)
            {
                itemsToAdd.Add(new ItemStack(item, 1));
            }
            _merchantInventory = new MerchantInventory(itemsToAdd, inventoryWidth, inventoryHeight);
        }
        
        public void LoadNpcModule()
        {

            ServiceLocator.Instance.Get<IMerchantInventoryService>().LoadMerchantInventory(_merchantInventory);
            _npc.AddMoreUIHandlers(ServiceLocator.Instance.Get<IMerchantInventoryService>() as IUIHandler);
        }

        public void UnloadNpcModule()
        {
            ServiceLocator.Instance.Get<IMerchantInventoryService>().CloseMerchantInventory();
            _npc.RemoveUIHandler(ServiceLocator.Instance.Get<IMerchantInventoryService>() as IUIHandler);
        }

        public NpcHandlerType HandlerType => NpcHandlerType.Merchant;
        public void Refresh()
        {
            //Refresh merchant inventory
            InitializeMerchantInventory();
        }
    }
}
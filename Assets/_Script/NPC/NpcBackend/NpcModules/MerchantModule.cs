// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 56

using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.ItemInstance;
using _Script.Inventory.MerchantInventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Managers;
using _Script.Managers.GlobalUpdater;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    public class MerchantModule : NpcModuleBase, IGlobalUpdate
    {
        #region NpcModuleBase

        public override NpcHandlerType HandlerType => NpcHandlerType.Merchant;
        public override string ModuleDescription => "Merchant Module";
        public override string ModuleName => "Show me your wares!";

        #endregion
        
        private MerchantInventory _merchantInventory;
        
        [SerializeField] private List<ItemData> itemsForSale;
        [SerializeField] private int inventoryWidth = 5;
        [SerializeField] private int inventoryHeight = 5;
        
        protected override void Awake()
        {
            base.Awake();
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
            var itemsToAdd = new List<ItemInstance>();
            foreach(var item in itemsForSale)
            {
                Debug.Log($"Adding item {item.ItemName} to merchant inventory.");
                itemsToAdd.Add(ItemInstanceFactory.CreateItemInstance(item, false, 1));
            }
            _merchantInventory = new MerchantInventory(itemsToAdd, inventoryWidth, inventoryHeight);
        }

        public override bool ShouldLoadModule()
        {
            return true;
        }

        public override void LoadNpcModule()
        {
            //Load merchant inventory UI
            ServiceLocator.Instance.Get<IMerchantInventoryService>().LoadMerchantInventory(_merchantInventory);
            //Register merchant inventory UI handler so it can be closed when the conversation ends
            Npc.AddMoreUIHandler(ServiceLocator.Instance.Get<IMerchantInventoryService>() as IUIHandler);
        }

        public override void UnloadNpcModule()
        {
            ServiceLocator.Instance.Get<IMerchantInventoryService>().CloseMerchantInventory();
            
            Npc.RemoveUIHandler(ServiceLocator.Instance.Get<IMerchantInventoryService>() as IUIHandler);
        }

        public void Refresh()
        {
            //Refresh merchant inventory
            InitializeMerchantInventory();
        }

        public override void OnLoadData(NpcSaveModule data)
        {
            if (data == null)
            {
                LoadDefaultData();
            }
            else
            {
                if(data is MerchantSaveModule mData)
                {
                    _merchantInventory = new MerchantInventory();
                    _merchantInventory.OnLoadData(mData.inventorySave.items);
                }
            }
            //Reload merchant inventory
            Refresh();
        }

        public override NpcSaveModule OnSaveData()
        {
            var saveInstance = new MerchantSaveModule
            {
                inventorySave = _merchantInventory.OnSaveData(),
            };
            
            return saveInstance;
        }
        
        public override void LoadDefaultData()
        {
            throw new NotImplementedException();
        }
    }
    
    [Serializable]
    public class MerchantSaveModule : NpcSaveModule
    {
        public InventorySave inventorySave;
    }
}
// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 56

using System.Collections.Generic;
using System.Linq;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.MerchantInventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    public class MerchantUnit : MonoBehaviour, INpcHandler
    {
        
        private MerchantInventory _merchantInventory;
        [SerializeField] private List<ItemData> itemsForSale;
        
        private void Awake()
        {
            
            //Add merchant inventory to merchant
            var itemsToAdd = new List<InventoryItem>();
            foreach(var item in itemsForSale)
            {
                itemsToAdd.Add(new InventoryItem(item, item.MaxStackSize));
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
    }
}
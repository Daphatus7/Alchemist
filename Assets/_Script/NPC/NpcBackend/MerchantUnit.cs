// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 56

using _Script.Inventory.MerchantInventoryBackend;
using _Script.Inventory.MerchantInventoryFrontend;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [RequireComponent(typeof(MerchantInventory))]
    public class MerchantUnit : MonoBehaviour, INpcHandler
    {
        
        private MerchantInventory _merchantInventory;
        
        private void Awake()
        {
            _merchantInventory = GetComponent<MerchantInventory>();
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
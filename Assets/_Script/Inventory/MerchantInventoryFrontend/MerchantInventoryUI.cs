// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 34

using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendBase;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.MerchantInventoryBackend;
using _Script.Inventory.SlotFrontend;
using _Script.Managers;
using _Script.NPC.NpcBackend;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;

namespace _Script.Inventory.MerchantInventoryFrontend
{
    public class MerchantInventoryUI : InventoryUIBase<MerchantInventory>, IMerchantInventoryService, IMerchantHandler   
    {
        public NpcHandlerType HandlerName => NpcHandlerType.Merchant;
        public SlotType SlotType => SlotType.Merchant;
        
        protected override void Start()
        {
            base.Start();
            ServiceLocator.Instance.Register<IMerchantInventoryService>(this);
        }
        
        private void OnDestroy()
        {
            if(ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<IMerchantInventoryService>();
        }        
        
        public void LoadMerchantInventory(MerchantInventory merchantInventory)
        {
            ShowUI();
            AssignInventory(merchantInventory);
            InitializeInventoryUI();
        }

        public void CloseMerchantInventory()
        {
            HideUI();
            ConversationManager.Instance.EndConversation();
        }
        
        
        public new void HideUI()
        {
            ClearInventory();
            base.HideUI();
        }
        public IUIHandler GetUIHandler()
        {
            return this;
        }

        public ItemStack Purchase(IPlayerInventoryHandler playerInventory, int slotIndex, int quantity = 1)
        {
            if(playerInventory.RemoveGold(_inventory.Slots[slotIndex].ItemData.Value * _inventory.Slots[slotIndex].Quantity))
            {
                return RemoveAllItemsFromSlot(slotIndex);;
            }
            return null;
        }
        
        
        
        /// <summary>
        /// Sell the item to the merchant.
        /// Add gold to the player's inventory and remove the item from the player's inventory.
        /// **Not** Removed from the player inventory
        /// </summary>
        /// <param name="playerInventory"></param>
        /// <param name="itemToSell"></param>
        /// <returns></returns>
        public bool Sell(IPlayerInventoryHandler playerInventory, InventorySlotDisplay itemToSell)
        {
            //check if the merchant accepts the trade
            if(AcceptTrade(itemToSell.ItemTypeName))
            {
                playerInventory.AddGold(itemToSell.Value * itemToSell.Quantity);
                return true;
            }
            return false;
        }

        public bool AcceptTrade(string itemTypeString)
        {
            return true;
        }

        public override bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }
    }
    

}
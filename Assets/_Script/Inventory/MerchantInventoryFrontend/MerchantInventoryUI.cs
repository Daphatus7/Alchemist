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
using UnityEngine;

namespace _Script.Inventory.MerchantInventoryFrontend
{
    public class MerchantInventoryUI : InventoryUIBase<MerchantInventory>, IMerchantInventoryService, IMerchantHandler   
    {
        public SlotType SlotType => SlotType.Merchant;
        
        protected override void Awake()
        {
        }
        
        protected override void Start()
        {
            ServiceLocator.Instance.Register<IMerchantInventoryService>(this);
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if(ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<IMerchantInventoryService>();
        }        
        
        public void LoadMerchantInventory(MerchantInventory merchantInventory)
        {
            AssignInventory(merchantInventory);
            InitializeInventoryUI();
            ShowUI();
        }

        public void CloseMerchantInventory()
        {
            //HideUI();
            ConversationManager.Instance.EndConversation();
        }


        private new void HideUI()
        {
            ClearInventory();
            base.HideUI();
        }
        public IUIHandler GetUIHandler()
        {
            return this;
        }

        public bool RemoveGold(IPlayerInventoryHandler playerInventory, ItemStack itemToSell, int quantity = 1)
        {
            return playerInventory.RemoveGold(itemToSell.ItemData.Value * itemToSell.Quantity);
        }

        /// <summary>
        /// Merchant Inventory, will replicate current item, if the item could not be added or in any case
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public override ItemStack RemoveAllItemsFromSlot(int slotIndex)
        {
            //should copy - use this if want to replenish the item for every purchase
            //return ItemStack.Copy(inventory.GetItemStackAt(slotIndex));
            //simply remove the item
            return base.RemoveAllItemsFromSlot(slotIndex);
        }
        
        /// <summary>
        /// Sell the item to the merchant.
        /// Add gold to the player's inventory and remove the item from the player's inventory.
        /// **Not** Removed from the player inventory
        /// </summary>
        /// <param name="playerInventory"></param>
        /// <param name="itemToSell"></param>
        /// <returns></returns>
        public bool Sell(IPlayerInventoryHandler playerInventory, ItemStack itemToSell)
        {
            //check if the merchant accepts the trade
            if(AcceptTrade(itemToSell.ItemData.ItemTypeString))
            {
                var price = itemToSell.ItemData.Value * itemToSell.Quantity;
                Debug.Log("Selling item for " + price + " gold.");
                playerInventory.AddGold(price);
                return true;
            }
            return false;
        }

        public bool AcceptTrade(string itemTypeString)
        {
            return true;
        }

        public bool CanAfford(IPlayerInventoryHandler player, ItemStack purchasedItem, int purchasedItemQuantity)
        {
            return player.GetGold() >= purchasedItem.ItemData.Value * purchasedItemQuantity;
        }

        public override bool AcceptsItem(ItemStack itemStack)
        {
            return true;
        }
    }
    

}
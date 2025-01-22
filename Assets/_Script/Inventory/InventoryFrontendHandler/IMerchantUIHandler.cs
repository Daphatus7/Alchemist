// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 36

using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.InventoryFrontendHandler
{
    public interface IMerchantHandler : IContainerUIHandle
    {
        bool RemoveGold(IPlayerInventoryHandler playerInventory, ItemStack itemToSell, int quantity = 1);
        bool Sell(IPlayerInventoryHandler playerInventory, ItemStack itemToSell);
        bool AcceptTrade(string itemTypeString);
        bool CanAfford(IPlayerInventoryHandler player, ItemStack purchasedItem, int purchasedItemQuantity);
    }
}
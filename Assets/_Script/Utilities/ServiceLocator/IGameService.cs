using _Script.Inventory.MerchantInventoryBackend;

namespace _Script.Utilities.ServiceLocator
{
    public interface IGameService
    {

    }
    
    public interface IMerchantInventoryService : IGameService
    {
        void LoadMerchantInventory(MerchantInventory merchantInventory);
        void CloseMerchantInventory();
    }
}

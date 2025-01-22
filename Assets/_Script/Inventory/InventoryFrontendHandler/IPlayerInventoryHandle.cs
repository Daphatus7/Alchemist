// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 57

namespace _Script.Inventory.InventoryFrontendHandler
{
    public interface IPlayerInventoryHandler : IContainerUIHandle
    {
        void AddGold(int amount);
        bool RemoveGold(int amount);
        int GetGold();
    }
}
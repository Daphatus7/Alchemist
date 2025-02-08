// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 41

using _Script.Alchemy.AlchemyTools;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;

namespace _Script.Alchemy
{
    public interface IAlchemyUIService : IUIService
    {
        void LoadAlchemyUI(PlayerAlchemy playerAlchemy, 
            Inventory.InventoryBackend.Inventory playerContainer, 
            AlchemyTool alchemyTool);
        IUIHandler GetUIHandler();
    }
}
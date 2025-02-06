// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 41

using _Script.Utilities.ServiceLocator;

namespace _Script.Alchemy
{
    public interface IAlchemyUIService : IUIService
    {
        void LoadAlchemyUI(PlayerAlchemy player, Cauldron.Cauldron cauldron);
    }
}
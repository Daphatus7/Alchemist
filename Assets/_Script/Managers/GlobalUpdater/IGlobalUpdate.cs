// Author : Peiyu Wang @ Daphatus
// 11 12 2024 12 17

namespace _Script.Managers.GlobalUpdater
{
    
    /// <summary>
    /// Only implement this if there is a need to update globally.
    /// Instances include MerchantInventory, etc.
    /// </summary>
    public interface IGlobalUpdate
    {
        void Refresh();
    }
}
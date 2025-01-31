// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 57

using _Script.UserInterface;

namespace _Script.NPC.NpcBackend
{
    public interface INpcModuleControlHandler
    {
        void AddMoreUIHandler(IUIHandler uiHandler);
        void RemoveUIHandler(IUIHandler uiHandler);
        string NpcId { get; }
    }
}
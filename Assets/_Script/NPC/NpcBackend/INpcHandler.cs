// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 28

namespace _Script.NPC.NpcBackend
{
    public interface INpcHandler
    {
        void LoadNpcModule();
        void UnloadNpcModule();
        
        NpcHandlerType HandlerType { get; }
    }
}
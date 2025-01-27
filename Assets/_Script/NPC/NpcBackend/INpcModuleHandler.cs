// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 28

using _Script.NPC.NpcBackend.NpcModules;

namespace _Script.NPC.NpcBackend
{
    public interface INpcModuleHandler
    {
        void LoadNpcModule();
        void UnloadNpcModule();
        
        NpcHandlerType HandlerType { get; }
    }
}
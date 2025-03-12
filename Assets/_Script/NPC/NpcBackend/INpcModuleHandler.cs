// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 28

using _Script.NPC.NpcBackend.NpcModules;

namespace _Script.NPC.NpcBackend
{
    public interface INpcModuleHandler
    {
        bool ShouldLoadModule();
        void LoadNpcModule();
        void UnloadNpcModule();
        NpcModuleInfo ModuleInfo { get; }
        NpcHandlerType HandlerType { get; }
    }
}
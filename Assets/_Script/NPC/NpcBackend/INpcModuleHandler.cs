// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 28

using _Script.NPC.NpcBackend.NpcModules;
using _Script.Quest;

namespace _Script.NPC.NpcBackend
{
    public interface INpcModuleHandler
    {
        void LoadNpcModule(INpcModuleHandler handler);
        void UnloadNpcModule(INpcModuleHandler handler);
        
        NpcModuleInfo ModuleInfo { get; }
        NpcHandlerType HandlerType { get; }
    }
    
    public interface INpcQuestModuleHandler : INpcModuleHandler
    {
        QuestDefinition CurrentQuest { get; }
    }
}
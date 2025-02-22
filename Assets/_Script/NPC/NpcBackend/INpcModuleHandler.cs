// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 28

using _Script.NPC.NpcBackend.NpcModules;
using _Script.Quest.QuestDefinition;
using _Script.Quest.QuestInstance;

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
    
    public interface INpcQuestModuleHandler : INpcModuleHandler
    {
        QuestInstance CurrentQuest { get; }
        QuestDefinition CurrentAvailableQuest { get; }
        bool StartQuest();
        void TryUnlockQuest();
        string NpcID { get; }
        bool CompleteQuest();
    }
}
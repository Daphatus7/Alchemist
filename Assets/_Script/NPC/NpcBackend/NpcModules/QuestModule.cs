// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using _Script.Quest;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    public class QuestGiverModule : NpcModuleBase, INpcQuestModuleHandler
    {
        
        [SerializeField] private string optionName = "Quest";
        
        [SerializeField] private QuestDefinition [] quests;
        [SerializeField] private QuestDefinition currentQuest;
        
        public override void LoadNpcModule(INpcModuleHandler handler)
        {
            //cast to QuestGiverUI and load the quest
            
        }

        public override void UnloadNpcModule(INpcModuleHandler handler)
        {
            Debug.Log("Quest Giver Module Unloaded");
        }
        
        public QuestDefinition CurrentQuest => currentQuest; 
        
        public override NpcHandlerType HandlerType => NpcHandlerType.QuestGiver;
        public override string ModuleDescription => "Quest Giver Module";
        public override string ModuleName => optionName;
    }
}

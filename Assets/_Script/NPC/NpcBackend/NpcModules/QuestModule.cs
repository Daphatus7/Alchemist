// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
using _Script.NPC.NPCFrontend;
using _Script.Quest;
using _Script.Utilities.ServiceLocator;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.NPC.NpcBackend.NpcModules
{
    public class QuestGiverModule : NpcModuleBase, INpcQuestModuleHandler
    {
        
        [SerializeField] private string optionName = "Quest";
        
        [SerializeField] private QuestDefinition [] quests;
        private Queue<QuestDefinition> _activeQuests;
        [SerializeField] private QuestDefinition currentQuest;

        public void Start()
        {
            _activeQuests = new Queue<QuestDefinition>();
            foreach (var quest in quests)
            {
                _activeQuests.Enqueue(quest);
            }
            if (_activeQuests.Count > 0)
                currentQuest = _activeQuests.Dequeue();
        }

        public override void LoadNpcModule(INpcModuleHandler handler)
        {
            ServiceLocator.Instance.Get<INpcUiCallback>().LoadQuestUi(this);
        }
        
        private QuestDefinition GetQuest()
        {
            return quests[Random.Range(0, quests.Length)];
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

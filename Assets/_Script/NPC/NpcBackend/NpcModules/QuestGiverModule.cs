// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
using _Script.NPC.NPCFrontend;
using _Script.Quest;
using _Script.Quest.QuestDefinition;
using _Script.Quest.QuestInstance;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    
    /// <summary>
    /// Quest Giver Module
    /// Data 
    /// </summary>
    public class QuestGiverModule : NpcModuleBase, INpcQuestModuleHandler
    {
        
        [SerializeField] private string optionName = "Quest";
        public string NpcID => NpcId;
  

        /// <summary>
        /// 列举所有npc能提供的任务
        /// 只有当前任务完成之后能够解锁下一个任务
        /// </summary>
        [SerializeField] private List<QuestDefinition> quests;
        private Queue<QuestDefinition> _quests; public Queue<QuestDefinition> QueuedQuests => _quests;
        private QuestInstance _currentQuest; public QuestInstance CurrentQuest => _currentQuest;
        private QuestDefinition _currentAvailableQuest; public QuestDefinition CurrentAvailableQuest => _currentAvailableQuest;
        #region for UI display
        public override NpcHandlerType HandlerType => NpcHandlerType.QuestGiver;
        public override string ModuleDescription => "Quest Giver Module";
        public override string ModuleName => optionName;
  

        #endregion
        
        public void Start()
        {
            _quests = new Queue<QuestDefinition>();
            foreach (var q in quests)
            {
                _quests.Enqueue(q);
            }
            
            TryUnlockQuest();
        }

        public bool StartQuest()
        {
            if (_currentAvailableQuest != null)
            {
                _currentQuest = new SideQuestInstance(_currentAvailableQuest);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void TryUnlockQuest()
        {
            //if there is already a quest, return
            if(_currentAvailableQuest != null) return;
            
            if (_quests.Count > 0)
            {
                var quest = _quests.Peek();
                if (quest.CanUnlockQuest())
                {
                    _currentAvailableQuest = _quests.Dequeue();
                }
            }
        }

        public override bool ShouldLoadModule()
        {
            //check if there is any active quest
            return _currentAvailableQuest != null;
        }
        
        public bool CompleteQuest()
        {
            //call the quest manager to complete the quest
            //check if the quest satisfies the completion condition
            if (!QuestManager.Instance.CheckQuestCompletion(_currentQuest))
            {
                Debug.Log("Quest not completed yet but displayed as completed");
            }
            else
            {
                QuestManager.Instance.CompleteQuest(_currentQuest);
                _currentQuest = null;
                _currentAvailableQuest = null;
                TryUnlockQuest();
                return true;
            }
            return false;
        }

        public override void LoadNpcModule()
        {
            ServiceLocator.Instance.Get<INpcUiCallback>().LoadQuestUi(this);
        }

        public override void UnloadNpcModule()
        {
        }

        #region Save and Load

        public override void OnLoadData(NpcSaveModule data)
        {
            //TODO: Implement this
        }

        public override NpcSaveModule OnSaveData()
        {
            //TODO: Implement this
            return null;
        }

        public override void LoadDefaultData()
        {
            //TODO: Implement this
        }

        #endregion
    }
}

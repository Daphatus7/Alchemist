// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 42

using System;
using System.Collections.Generic;
using _Script.Quest.QuestDef;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Quest.PlayerQuest
{
    public sealed class PlayerQuestManager : MonoBehaviour, IPlayerQuestService
    {
        private readonly Dictionary<string, QuestInstance> _activeQuests = new Dictionary<string, QuestInstance>(); 
        private readonly Queue<QuestInstance> _completedQuests = new Queue<QuestInstance>(); public Queue<QuestInstance> CompletedQuests => _completedQuests;
        private GuildQuestInstance _activeGuildQuest;
        public void Update()
        {
            //Temporary only for testing
            if (!Prototype_Active_Quest_Ui.Instance) return;
            Prototype_Active_Quest_Ui.Instance.SetText("");
            string t = "";
            foreach (var quest in _activeQuests)
            {
                t += quest.Value.QuestStatus + "\n";
            } 
            t += "-----------------\n";
            if (_activeGuildQuest != null)
            {
                t += "Guild Quest: \n" + _activeGuildQuest.QuestStatus;
            }
            Prototype_Active_Quest_Ui.Instance.SetText(t);
        }
        
        public void OnEnable()
        {
            ServiceLocator.Instance?.Register<IPlayerQuestService>(this);
        }

        public void OnDisable()
        {
            if (!Application.isPlaying) return;

            // Check if ServiceLocator still exists
            if (ServiceLocator.Instance == null) return;

            var playerQuestService = ServiceLocator.Instance.Get<IPlayerQuestService>();
            if ((PlayerQuestManager)playerQuestService == this)
            {
                ServiceLocator.Instance.Unregister<IPlayerQuestService>();
            }
        }
        
        public void UpdateQuestState(QuestInstance quest)
        {
            
        }

        public void AddNewQuest(QuestInstance quest)
        {
            if(!_activeQuests.TryAdd(quest.QuestDefinition.questID, quest)) return;
        }

        public bool CompleteQuest(string questId)
        {
            //check list of active quests
            if (!_activeQuests.TryGetValue(questId, out var quest)) return false;
            
            //if quest is found, complete the quest
            CompleteQuest(quest);
            return true;
        }
        
        public bool CompleteGuildQuest(string questId)
        {
            if (_activeGuildQuest == null) return false;
            if (_activeGuildQuest.QuestDefinition.questID != questId) return false;
            CompleteGuildQuest(_activeGuildQuest);
            return true;
        }
        public void AddNewGuildQuest(GuildQuestInstance quest)
        {
            _activeGuildQuest = quest;
        }

        private void CompleteGuildQuest(GuildQuestInstance quest)
        {
            Debug.Log("Guild Quest Completed");
            _activeGuildQuest = null;
        }
        
        
        public void AddNewSideQuest(QuestInstance quest)
        {
            if(!_activeQuests.TryAdd(quest.QuestDefinition.questID, quest)) return;
        }
        
        public void CompleteQuest(QuestInstance quest)
        {
            //issue right now, guild quest is different
            if(quest == null) return;
            switch (quest.QuestType)
            {
                case QuestType.Main:
                    break;
                case QuestType.Side:
                    CompleteSideQuest(quest);
                    break;
                case QuestType.Guild:
                    CompleteGuildQuest((GuildQuestInstance) quest);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CompleteSideQuest(QuestInstance quest)
        {
            _completedQuests.Enqueue(quest);
            if (!_activeQuests.Remove(quest.QuestDefinition.questID)) return;
        }
    }
}
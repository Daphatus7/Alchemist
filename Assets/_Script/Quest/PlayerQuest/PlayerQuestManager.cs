// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 42

using System;
using System.Collections.Generic;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Quest.PlayerQuest
{
    public sealed class PlayerQuestManager : MonoBehaviour, IPlayerQuestService
    {
        private readonly Dictionary<string, QuestInstance> _activeQuests = new Dictionary<string, QuestInstance>(); 
        private readonly Queue<QuestInstance> _completedQuests = new Queue<QuestInstance>(); public Queue<QuestInstance> CompletedQuests => _completedQuests;
        
        public void Update()
        {
            if (!Prototype_Active_Quest_Ui.Instance) return;
            Prototype_Active_Quest_Ui.Instance.SetText("");
            foreach (var quest in _activeQuests)
            {
                Prototype_Active_Quest_Ui.Instance.SetText(quest.Value.QuestStatus);
            }
        }
        
        public void OnEnable()
        {
            Debug.Log(ServiceLocator.Instance + " " + this);
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
        
        
        public void AddNewSideQuest(QuestInstance quest)
        {
            if(!_activeQuests.TryAdd(quest.QuestDefinition.questID, quest)) return;
        }
        
        public void CompleteQuest(QuestInstance quest)
        {
            _completedQuests.Enqueue(quest);
            if(!_activeQuests.Remove(quest.QuestDefinition.questID)) return;
        }
    }
}
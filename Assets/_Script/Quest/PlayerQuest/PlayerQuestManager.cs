// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 42

using System;
using System.Collections.Generic;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Quest.PlayerQuest
{
    [DefaultExecutionOrder(0)]
    public sealed class PlayerQuestManager : MonoBehaviour, IPlayerQuestService
    {
        private List<QuestInstance> _activeQuests; public List<QuestInstance> ActiveQuests => _activeQuests;
        private List<MainQuestInstance> _mainQuests; public List<MainQuestInstance> MainQuests => _mainQuests;
        private Queue<QuestInstance> _completedQuests; public Queue<QuestInstance> CompletedQuests => _completedQuests;


        public void Awake()
        {
            
            ServiceLocator.Instance.Register<IPlayerQuestService>(this);
        }

        public void OnDestroy()
        {
            ServiceLocator.Instance.Unregister<IPlayerQuestService>();
        }

        public void AddNewQuest(QuestInstance quest)
        {
            _activeQuests.Add(quest);
        }
        
        public void AddMainQuest(MainQuestInstance quest)
        {
            _mainQuests.Add(quest);
        }
        
        public void AddNewSideQuest(QuestInstance quest)
        {
            _activeQuests.Add(quest);
        }
        
        public void CompleteQuest(QuestInstance quest)
        {
            _completedQuests.Enqueue(quest);
            _activeQuests.Remove(quest);
        }
    }
}
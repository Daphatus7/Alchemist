// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 42

using System;
using System.Collections.Generic;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Quest.PlayerQuest
{
    public sealed class PlayerQuestManager : MonoBehaviour, IPlayerQuestService
    {
        private readonly List<QuestInstance> _activeQuests = new List<QuestInstance>(); public List<QuestInstance> ActiveQuests => _activeQuests;
        private readonly List<MainQuestInstance> _mainQuests = new List<MainQuestInstance>(); public List<MainQuestInstance> MainQuests => _mainQuests;
        private readonly Queue<QuestInstance> _completedQuests = new Queue<QuestInstance>(); public Queue<QuestInstance> CompletedQuests => _completedQuests;

        
        public void Start()
        {
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
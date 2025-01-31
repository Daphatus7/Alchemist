// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 14

using System;
using System.Collections.Generic;
using _Script.Managers;
using _Script.NPC.NpcBackend;
using _Script.NPC.NpcBackend.NpcModules;
using UnityEngine;

namespace _Script.Quest
{
    
    public sealed class QuestManager : Singleton<QuestManager>
    {
        private List<QuestInstance> _activeQuests; public List<QuestInstance> ActiveQuests => _activeQuests;
        
        /// <summary>
        /// NPC ID -> Quest Giver Module
        /// </summary>
        private Dictionary<string, INpcQuestModuleHandler> _questModules;
        public event Action<string> onEnemyKilled;
        public event Action<string, int> onItemCollected;
        public event Action<QuestInstance> onQuestCompleted;

        [SerializeField] public StorylineChecker storylineChecker;
        
        public void Start()
        {
            InitializeNpcModules();
        }
        
        /// <summary>
        /// Collect all the quest giver modules
        /// </summary>
        private void InitializeNpcModules()
        {
            _questModules = new Dictionary<string, INpcQuestModuleHandler>();
            
            var questModules = FindObjectsByType<QuestGiverModule>(FindObjectsSortMode.None);
            foreach (var o  in questModules)
            {
                _questModules.Add(o.NpcId, o);
            }
        }
        
        
        /// <summary>
        /// this could be called when certain event happens
        /// </summary>
        private void UpdateNpcModules()
        {
            foreach (var module in _questModules)
            {
                module.Value.TryUnlockQuest();
            }
        }
        

        /// <summary>
        /// Triggered when an item is collected
        /// Should be called by the inventory manager
        /// </summary>
        /// <param name="itemID"> item ID</param>
        /// <param name="totalCount"> the number of items in the inventory</param>
        public void OnItemCollected(string itemID, int totalCount)
        {
            Debug.Log($"[QuestManager] Item collected: {itemID}");
            onItemCollected?.Invoke(itemID,totalCount);
        }

        /// <summary>
        /// Happens when an enemy is killed
        /// </summary>
        /// <param name="enemyID"></param>
        public void OnEnemyKilled(string enemyID)
        {
            onEnemyKilled?.Invoke(enemyID);
        }

        private void CheckQuestCompletion(QuestInstance quest)
        {
            if (quest.TryCompleteQuest())
            {
                //remove items from inventory etc.
            }
        }

        private void GiveReward(QuestReward reward)
        {
        }

        public bool CheckPrerequisite(UnlockCondition prerequisite)
        {
            //check if the player has the required rank
            return GameManager.Instance.PlayerRank >= prerequisite.playerRank &&
                   //check if the player has completed the required quest
                   storylineChecker.IsCompleted(prerequisite.prerequisite);
        }

        private void OnOnQuestCompleted(QuestInstance quest)
        {
            onQuestCompleted?.Invoke(quest);
        }
    }

}
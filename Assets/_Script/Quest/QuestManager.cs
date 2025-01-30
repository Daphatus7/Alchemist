// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 14

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Character.PlayerRank;
using _Script.Quest.PlayerQuest;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
namespace _Script.Quest
{
    
    public sealed class QuestManager : Singleton<QuestManager>
    {
        public event Action<string> onEnemyKilled;
        public event Action<string, int> onItemCollected;
        
        
        public void StartQuest(QuestDefinition questDef)
        {
            ServiceLocator.Instance.Get<IPlayerQuestService>().AddNewSideQuest(new SideQuestInstance(questDef));
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
    }

}
// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 14

using System;
using System.Collections.Generic;
using _Script.Items.Lootable;
using _Script.Managers;
using _Script.NPC.NpcBackend;
using _Script.NPC.NpcBackend.NpcModules;
using UnityEditor.PackageManager;
using UnityEngine;

namespace _Script.Quest
{
    
    public sealed class QuestManager : Singleton<QuestManager>
    {
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

        public bool CheckQuestCompletion(QuestInstance quest)
        {
            // Iterate over each objective in the quest.
            foreach (var objective in quest.Objectives)
            {
                // If any objective is not marked as complete, the quest is not complete.
                if (!objective.isComplete)
                {
                    Debug.Log($"Objective {objective.objectiveData.type} is not complete");
                    return false;
                }

                // If the objective is a "Collect" type, verify the inventory count.
                if (objective.objectiveData.type == ObjectiveType.Collect)
                {
                    var collectObjective = (CollectObjective)objective.objectiveData;
                    var count = GameManager.Instance.PlayerCharacter.PlayerInventory.GetItemCount(collectObjective.item.itemID);
                    // If the inventory count is less than required, the objective is not satisfied.
                    if (count < objective.objectiveData.requiredCount)
                    {
                        return false;
                    }
                }
        
                // For other objective types, you might add additional checks here if needed.
            }
    
            // If we get through all objectives without returning false, then the quest is complete.
            return true;
        }

        
        public void CompleteQuest(QuestInstance currentQuest)
        {
            var questObjectives = currentQuest.Objectives;
            foreach (var o in questObjectives)
            {
                if (o.isComplete)
                {
                    if (o.objectiveData.type == ObjectiveType.Collect)
                    {
                        //remove the items from the player inventory
                        GameManager.Instance.PlayerCharacter.PlayerInventory.RemoveItemById(((CollectObjective)o.objectiveData).item.itemID, o.objectiveData.requiredCount);
                    }
                }
                else
                {
                    throw new Exception("Quest is not completed, but it was claimed to be completed");
                    return;
                }
            }
            GiveReward(currentQuest.QuestDefinition.reward);
            currentQuest.Cleanup();
        }
        
        private void GiveReward(QuestReward reward)
        {
            //Drop the reward items
            foreach (var o in reward.items)
            {
                //Debug the item drop
                Debug.Log($"[QuestManager] Dropping {o.amount} {o.item.itemID}");
                ItemLootable.DropItem(GameManager.Instance.PlayerCharacter.transform.position, o.item, o.amount);
            }
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